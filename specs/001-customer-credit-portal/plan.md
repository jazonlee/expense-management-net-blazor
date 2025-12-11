# Implementation Plan: Customer Credit Portal

**Branch**: `001-customer-credit-portal` | **Date**: 2025-12-10 | **Spec**: `specs/001-customer-credit-portal/spec.md`
**Input**: Feature specification for the Customer Credit Portal (dashboard, invoices, invoice detail, payments, and SOA).

**Note**: This plan is generated via `/speckit.plan` and must remain consistent with the project constitution in `.specify/memory/constitution.md`.

## Summary

Deliver a Blazor Server–based Customer Credit Portal that surfaces a credit
overview dashboard, invoice browsing and detail views, payment history, and
statement-of-account (SOA) downloads for a single customer account per
authenticated user. Implementation will follow Clean Architecture using
`CustomerPortal.Core` for domain/application logic and `CustomerPortal.Web` for
UI, API-style services, and infrastructure (Dapper repositories and
Unit-of-Work over SQL Server Express). Filtering and pagination will be
server-side via Dapper, and all UI will be implemented by porting the
HTML/CSS from `/design_assets` 1:1 into Razor components while preserving
Bootstrap 5 classes and CSS variables.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: .NET 9 (C#) with Blazor Server  
**Primary Dependencies**: ASP.NET Core Blazor Server, Dapper, SQLClient, Bootstrap 5, Bootstrap Icons, QuestPDF  
**Storage**: SQL Server Express (relational, normalized around Customers, Invoices, InvoiceItems, Payments)  
**Testing**: xUnit for domain and repository logic; optional bUnit for Blazor components in later phases  
**Target Platform**: ASP.NET Core on Windows/Linux server; modern desktop and mobile browsers consuming the Blazor Server UI  
**Project Type**: Web application (Blazor Server UI + internal service layer)  
**Performance Goals**: Dashboard and list filters (invoices, payments) should respond within ~2 seconds in at least 90% of interactions, consistent with spec success criteria; server must avoid blocking I/O on database calls.  
**Constraints**: Dapper-only data access, async/await for all I/O, SQL Server Express as backing store, strict design compliance to `/design_assets` with WCAG AA, responsive layout with sidebar collapsing < 992px.  
**Scale/Scope**: Single-customer-per-user portal, thousands of invoices and payments per customer, initial focus on one business unit; feature scope includes dashboard, invoices, payments, and SOA with offline payments (no real-time gateway), and no multi-currency or advanced admin tooling.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Tech Stack & Frameworks**: Uses .NET 9 Blazor Server, SQL Server Express,
  and Dapper-only data access with Bootstrap 5 and Bootstrap Icons, fully
  aligned with the constitution.
- **Architecture Guidelines**: Plan defines clear Domain/Application logic in
  `CustomerPortal.Core` and Infrastructure/API/UI layers in
  `CustomerPortal.Web` via folders (`Infrastructure`, `Services`, `Pages`,
  `Components`), honoring Clean Architecture boundaries and Unit of Work over
  Dapper repositories.
- **Coding Standards**: All repository and service methods will be async,
  using parameterized SQL only; C# naming conventions (PascalCase/camelCase)
  will be enforced; no synchronous DB calls are planned.
- **Design Compliance**: All pages (Dashboard, Invoices, Invoice Detail,
  Payments & SOA) will be built by porting the HTML structure and CSS classes
  from `/design_assets` 1:1 into Razor, using CSS variables (for example,
  `--primary-blue`, `--text-muted`) via `app.css`, ensuring WCAG AA contrast
  and responsive behavior.

Result: **PASS** – no constitution violations or exceptions requested for this
feature. Complexity Tracking remains empty.

## Project Structure

### Documentation (this feature)

```text
specs/001-customer-credit-portal/
├── spec.md              # Feature specification (user stories, FRs, SCs)
├── plan.md              # This file (/speckit.plan output)
├── research.md          # Phase 0: design decisions and trade-offs
├── data-model.md        # Phase 1: entities, fields, relationships
├── quickstart.md        # Phase 1: how to run and exercise feature
├── contracts/           # Phase 1: OpenAPI contracts for internal services
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2: implementation tasks (/speckit.tasks)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
CustomerPortal.sln

CustomerPortal.Core/
├── Domain/
│   ├── Customers/
│   ├── Invoices/
│   ├── Payments/
│   └── Shared/
└── Application/
  ├── DTOs/
  ├── Interfaces/
  │   ├── IInvoiceRepository.cs
  │   ├── IPaymentRepository.cs
  │   └── IUnitOfWork.cs
  └── Services/          # Optional pure application services

CustomerPortal.Web/
├── Infrastructure/
│   ├── Persistence/
│   │   ├── SqlConnectionFactory.cs
│   │   ├── InvoiceRepository.cs
│   │   ├── PaymentRepository.cs
│   │   └── UnitOfWork.cs
│   └── Migrations/        # SQL scripts and seed data for Invoices, InvoiceItems, Payments, Customers
├── Services/              # API-style services: InvoiceService, PaymentService, DashboardService
├── Api/                   # Optional controllers/endpoints if REST API is exposed
├── Pages/
│   ├── Index.razor        # Redirects to Dashboard
│   ├── Dashboard.razor
│   ├── Invoices.razor
│   ├── InvoiceDetail.razor
│   └── Payments.razor
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor
│   ├── Dashboard/
│   │   ├── StatsCard.razor
│   │   └── ActivityList.razor
│   ├── Invoices/
│   │   ├── InvoiceFilter.razor
│   │   └── InvoiceTable.razor
│   └── Payments/
│       └── PaymentTable.razor
└── wwwroot/
  └── css/
    └── app.css        # CSS variables and styles ported from /design_assets

tests/
└── CustomerPortal.Tests/
  ├── Repositories/
  │   └── InvoiceRepositoryTests.cs
  └── Services/
    └── DashboardServiceTests.cs
```

**Structure Decision**: Two-project solution (`CustomerPortal.Core`,
`CustomerPortal.Web`) implementing logical Domain/Application/Infrastructure/API/UI
layers via folders. This aligns with the constitution while keeping the
solution small and focused for this feature.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
