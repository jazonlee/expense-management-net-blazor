# Phase 0 Research: Customer Credit Portal

**Feature**: 001-customer-credit-portal  
**Date**: 2025-12-10

This document captures key technical decisions, rationale, and alternatives for the
Customer Credit Portal feature prior to detailed design and implementation.

---

## Decision 1: Tech Stack & Layering

- **Decision**: Implement the portal as a .NET 9 Blazor Server web application
  with two projects: `CustomerPortal.Core` for Domain/Application logic and
  `CustomerPortal.Web` for Infrastructure, API-style services, and UI (Blazor
  components and pages).
- **Rationale**: This matches the constitution’s requirements (.NET 9, Blazor
  Server, SQL Server Express, Dapper-only) while keeping the solution small.
  Domain/Application layers live in `CustomerPortal.Core` and can be tested in
  isolation; `CustomerPortal.Web` hosts the Blazor UI, Dapper repositories,
  Unit of Work, and any REST-style endpoints needed later.
- **Alternatives considered**:
  - **Multiple class libraries (Domain, Application, Infrastructure)**: Offers
    stricter layering via project boundaries but adds solution complexity for a
    single-portal feature.
  - **Single Blazor project only**: Simpler initially but violates the
    constitution’s separation between Domain/Application and Infrastructure/UI.

---

## Decision 2: Data Access & Database Schema

- **Decision**: Use Dapper with `SqlConnection` against SQL Server Express.
  Define tables `Customers`, `Invoices`, `InvoiceItems`, and `Payments`, plus
  supporting views for dashboard activity and SOA, with all access routed
  through repositories behind `IUnitOfWork`.
- **Rationale**: Dapper aligns with the constitution (no EF Core) and gives
  explicit control over queries and joins, which is important for dashboard and
  filtering views. A `UnitOfWork` coordinating repositories simplifies
  transactions when payments affect invoices and activity.
- **Alternatives considered**:
  - **Entity Framework Core**: Rejected by constitution (ORM constraint).
  - **Ad-hoc ADO.NET everywhere**: Too low-level and error-prone compared to
    consolidating patterns in Dapper-based repositories.

---

## Decision 3: Current Customer Context (FR-019)

- **Decision**: Each authenticated portal user is associated with exactly one
  customer account; all queries are implicitly scoped by this customer ID
  resolved from the user’s identity or session.
- **Rationale**: Simplifies authorization and UI—no account switching UI, and
  every query can inject a single `@CustomerId` parameter. This matches the
  typical "single account per customer login" model for customer credit
  portals.
- **Alternatives considered**:
  - **Multi-account selection per user**: Adds account-switch UI and more edge
    cases (no account selected, last-selected account). Considered out of scope
    for this first release.
  - **External context only (host sets customer)**: Would tightly couple this
    feature to a specific embedding pattern; keeping the resolution in the
    portal affords more flexibility.

---

## Decision 4: Pay Now Behaviour (FR-020)

- **Decision**: "Pay Now" will mark the invoice as paid based on an offline or
  external process rather than processing a real-time payment within this
  portal.
- **Rationale**: The spec focuses on visibility and statement management rather
  than live payment gateway integration. Treating Pay Now as a request that
  transitions invoice status (for example, to "Payment In Progress" or
  "Paid-Offline") avoids PCI and gateway complexities at this stage and keeps
  the work aligned with the initial milestones.
- **Alternatives considered**:
  - **Redirect to existing corporate payments portal**: Viable but requires a
    stable external URL and hand-off contract (query parameters, return URLs)
    that are not yet defined.
  - **Embed a third-party payment gateway (cards/ACH)**: Richer UX but much
    larger scope (PCI, tokenization, webhooks). Better suited for a dedicated
    payment-integration feature.

---

## Decision 5: SOA Default Period (FR-021)

- **Decision**: The Statement of Account (SOA) will default to the **current
  calendar month** when the user clicks "Download SOA (PDF)", with no period
  selector in the initial version.
- **Rationale**: A single, predictable default aligns with how finance teams
  typically reconcile statements and simplifies UI. Future features can introduce
  period selection or historical statements if needed.
- **Alternatives considered**:
  - **Last 30 days**: Misaligned with calendar-based reconciliation and billing
    cycles.
  - **Configurable billing cycles with selectable periods**: More flexible but
    requires additional configuration and UI complexity not justified for the
    initial release.

---

## Decision 6: Filtering & Pagination Strategy

- **Decision**: Implement all filtering (invoice number, status, amount range;
  payment date range and method) and pagination on the server via Dapper
  queries with WHERE and ORDER BY clauses, using OFFSET/FETCH for paging.
- **Rationale**: Server-side filtering and paging keep queries efficient when
  invoice and payment counts grow to thousands per customer, avoids transferring
  large datasets to the browser, and keeps logic close to the data model.
- **Alternatives considered**:
  - **Client-side filtering over full dataset**: Simpler but does not scale and
    conflicts with performance expectations.
  - **Stored procedures for all queries**: Could be used later for hotspots but
    are not necessary for the initial implementation; inline parameterized SQL
    keeps logic visible and easier to change.

---

## Decision 7: Blazor UI & Design Assets

- **Decision**: All Blazor pages and components (Dashboard, Invoices, Invoice
  Detail, Payments & SOA, layout and shared components) will be built by
  porting HTML from `/design_assets` 1:1 into `.razor` files, preserving the
  DOM structure and classes, and centralizing theming via CSS variables in
  `app.css`.
- **Rationale**: This strictly follows the constitution’s design compliance
  rules, ensures visual parity with mockups, and keeps accessibility (WCAG AA)
  consistent. It also reduces design drift over time.
- **Alternatives considered**:
  - **Hand-crafted Blazor markup from scratch**: Higher risk of design drift
    and inconsistent styling.
  - **Using a different component library**: Conflicts with Bootstrap 5 and
    Bootstrap Icons requirements and would introduce unnecessary complexity.

---

## Decision 8: Testing Approach

- **Decision**: Start with xUnit tests focused on repository filtering logic
  (for example, `InvoiceRepository` queries) and critical service logic (for
  example, `DashboardService` aggregation). UI testing via bUnit can be added
  in later phases if needed.
- **Rationale**: Repository filtering is both critical and easy to regress as
  new filters are added. xUnit integrates well with .NET and keeps the test
  story simple for the first milestones.
- **Alternatives considered**:
  - **No automated tests in initial milestones**: Faster to start but higher
    risk of regressions as filters and calculations evolve.
  - **Full bUnit coverage from day one**: Stronger safety net but higher
    upfront cost; better introduced incrementally after core flows stabilize.
