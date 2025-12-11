# Tasks: Customer Credit Portal

**Input**: Design documents from `specs/001-customer-credit-portal/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`

**Tests**: Automated tests are focused on critical repository and service logic (especially invoice filtering and dashboard aggregation). Additional UI tests can be added later if needed.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Initialize solution and projects `CustomerPortal.sln`, `CustomerPortal.Core/CustomerPortal.Core.csproj`, `CustomerPortal.Web/CustomerPortal.Web.csproj`
- [x] T002 Create Clean Architecture folder structure in `CustomerPortal.Core/Domain` and `CustomerPortal.Core/Application` and `CustomerPortal.Web/Infrastructure`, `CustomerPortal.Web/Services`, `CustomerPortal.Web/Pages`, `CustomerPortal.Web/Components`
- [x] T003 Configure SQL Server Express connection string in `CustomerPortal.Web/appsettings.json`
- [x] T004 Wire Blazor and dependency injection setup in `CustomerPortal.Web/Program.cs` (register connection factory, repositories, UnitOfWork, and services)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T005 Create domain entity classes `Customer`, `Invoice`, `InvoiceItem`, `Payment`, `Activity`, `Notification` in `CustomerPortal.Core/Domain`
- [x] T005 Create domain entity classes `Customer`, `Invoice`, `InvoiceItem`, `Payment`, `Activity`, `Notification` in `CustomerPortal.Core/Domain`
- [x] T006 [P] Add DTOs for dashboard, invoices, payments, and SOA (e.g., `DashboardSummaryDto`, `ActivityDto`, `InvoiceListItemDto`, `InvoiceDetailDto`, `PaymentDto`) in `CustomerPortal.Core/Application/DTOs`
- [x] T007 [P] Define `IInvoiceRepository`, `IPaymentRepository`, and `IUnitOfWork` interfaces in `CustomerPortal.Core/Application/Interfaces`
- [x] T008 Create SQL schema and seed scripts for `Customers`, `Invoices`, `InvoiceItems`, `Payments` in `CustomerPortal.Web/Infrastructure/Migrations`
- [x] T009 Implement `SqlConnectionFactory` for SQL Server Express in `CustomerPortal.Web/Infrastructure/Persistence/SqlConnectionFactory.cs`
- [x] T010 Implement base `UnitOfWork` handling transactions and repository access in `CustomerPortal.Web/Infrastructure/Persistence/UnitOfWork.cs`
- [x] T011 Create test project `CustomerPortal.Tests/CustomerPortal.Tests.csproj` and add references to `CustomerPortal.Core` and `CustomerPortal.Web`
- [x] T012 [P] Add xUnit test fixture setup for database/in-memory testing in `CustomerPortal.Tests/Repositories/InvoiceRepositoryTests.cs`

**Checkpoint**: Foundation ready – repositories, UnitOfWork, schema, and basic tests in place; user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - View credit overview & recent activity (Priority: P1) 🎯 MVP

**Goal**: Allow a signed-in customer to see Available Credit, Credit Limit, Due Balance, recent activity, and notices on the dashboard.

**Independent Test**: Sign in as a test customer and load `Dashboard.razor`; verify that credit stats match seeded data, the activity list shows recent invoices/payments chronologically, and any active notifications appear as cards, without visiting other pages.

### Implementation for User Story 1

- [x] T013 [P] [US1] Implement `DashboardService` to compute summary and fetch activity in `CustomerPortal.Web/Services/DashboardService.cs`
- [x] T014 [P] [US1] Add Dapper queries for dashboard summary and recent activity in `CustomerPortal.Web/Infrastructure/Persistence/InvoiceRepository.cs` and `CustomerPortal.Web/Infrastructure/Persistence/PaymentRepository.cs`
- [x] T015 [P] [US1] Create layout component `CustomerPortal.Web/Components/Layout/MainLayout.razor` by porting sidebar and header from `/design_assets/dashboard.html`
- [x] T016 [P] [US1] Create `CustomerPortal.Web/Pages/Dashboard.razor` by porting dashboard content from `/design_assets/dashboard.html` and binding to `DashboardService`
- [x] T017 [P] [US1] Create `CustomerPortal.Web/Components/Dashboard/StatsCard.razor` from the stats cards section in `/design_assets/dashboard.html`
- [x] T018 [P] [US1] Create `CustomerPortal.Web/Components/Dashboard/ActivityList.razor` from the activity list table in `/design_assets/dashboard.html`
- [x] T019 [US1] Wire sidebar navigation links in `CustomerPortal.Web/Components/Layout/MainLayout.razor` to `Dashboard.razor`, `Invoices.razor`, and `Payments.razor`
- [x] T020 [P] [US1] Add xUnit tests for `DashboardService` aggregation logic in `CustomerPortal.Tests/Services/DashboardServiceTests.cs`

**Checkpoint**: User Story 1 fully functional and testable; dashboard alone should provide a meaningful MVP if other stories are deferred.

---

## Phase 4: User Story 2 - Browse & filter invoices (Priority: P2)

**Goal**: Allow customers to browse invoices with filters (number, status, amount range) and pagination to quickly locate specific invoices.

**Independent Test**: From the dashboard, navigate to `Invoices.razor`; apply different combinations of filters and paginate through results to confirm the list updates correctly, independent of invoice detail or payments pages.

### Implementation for User Story 2

- [x] T021 [P] [US2] Add invoice list and paging DTOs (e.g., `InvoiceListItemDto`, `PagedInvoiceListDto`) in `CustomerPortal.Core/Application/DTOs`
- [x] T022 [P] [US2] Implement filtered and paged invoice query using Dapper in `CustomerPortal.Web/Infrastructure/Persistence/InvoiceRepository.cs`
- [x] T023 [US2] Implement `GetInvoicesAsync` in `CustomerPortal.Web/Services/InvoiceService.cs` to expose filter and paging options to the UI
- [x] T024 [P] [US2] Create `CustomerPortal.Web/Components/Invoices/InvoiceFilter.razor` by porting the filter form from `/design_assets/invoices.html`
- [x] T025 [P] [US2] Create `CustomerPortal.Web/Components/Invoices/InvoiceTable.razor` by porting the invoices table and pagination UI from `/design_assets/invoices.html`
- [x] T026 [US2] Create `CustomerPortal.Web/Pages/Invoices.razor` composing `InvoiceFilter` and `InvoiceTable` and binding to `InvoiceService`

**Checkpoint**: User Story 2 allows independent exploration and filtering of invoices, reusing foundation and US1 components but not depending on User Story 3 or 4.

---

## Phase 5: User Story 3 - Inspect invoice details & actions (Priority: P3)

**Goal**: Allow customers to open an invoice, see full header, addresses, line items, and summary (Subtotal, Discount, Tax @8%, Total), and access actions to print, download PDF, or mark as paid (offline Pay Now).

**Independent Test**: From `Invoices.razor`, open an invoice detail route and verify that all header, address, line item, and summary fields are correct and that the three actions behave as specified (print stub, PDF download or stub, Pay Now marking the invoice as paid/offline).

### Implementation for User Story 3

- [x] T027 [P] [US3] Extend DTOs for invoice detail (including bill-to/ship-to blocks and line items) in `CustomerPortal.Core/Application/DTOs`
- [x] T028 [P] [US3] Implement `GetInvoiceByIdAsync` with header, addresses, and line items in `CustomerPortal.Web/Infrastructure/Persistence/InvoiceRepository.cs`
- [x] T029 [US3] Implement `GetInvoiceDetailAsync` and `MarkInvoicePaidOfflineAsync` methods in `CustomerPortal.Web/Services/InvoiceService.cs`
- [x] T030 [P] [US3] Create `CustomerPortal.Web/Pages/InvoiceDetail.razor` with route `@page "/invoices/{id}"` by porting layout from `/design_assets/invoice-detail.html`
- [x] T031 [US3] Wire Print, Download PDF, and Pay Now button handlers in `CustomerPortal.Web/Pages/InvoiceDetail.razor` (Print + PDF can initially stub or call placeholder endpoints; Pay Now calls offline payment method)

**Checkpoint**: User Story 3 enables deep inspection of invoices and manual actions, independently testable with seeded data.

---

## Phase 6: User Story 4 - Review payments & download SOA (Priority: P4)

**Goal**: Allow customers to review payment history filtered by date range and method, and download a PDF statement of account for the current calendar month.

**Independent Test**: Navigate to `Payments.razor`, filter by various date ranges and methods, and verify table results; click "Download SOA (PDF)" and confirm a statement is returned for the current calendar month.

### Implementation for User Story 4

- [x] T032 [P] [US4] Ensure `PaymentDto` and any SOA-related DTOs are defined in `CustomerPortal.Core/Application/DTOs`
- [x] T033 [P] [US4] Implement filtered payment history query in `CustomerPortal.Web/Infrastructure/Persistence/PaymentRepository.cs`
- [x] T034 [US4] Implement `GetPaymentsAsync` and **SOA statement generation** in `CustomerPortal.Web/Services/PaymentService.cs`, including a method that builds statement entries and renders a PDF using QuestPDF for the current calendar month
- [x] T035 [P] [US4] Create `CustomerPortal.Web/Pages/Payments.razor` by porting layout from `/design_assets/payments.html` with date range and method filters bound to `PaymentService`
- [x] T036 [US4] Wire "Download SOA (PDF)" button in `CustomerPortal.Web/Pages/Payments.razor` to a real SOA PDF endpoint (for example, `/soa/pdf`) that uses `PaymentService` to generate and return the PDF for the current statement period

**Checkpoint**: User Story 4 completes the payments and SOA experience, independently testable via its own page.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and harden the feature for real use

- [ ] T037 [P] Refine CSS in `CustomerPortal.Web/wwwroot/css/app.css` for mobile breakpoints (especially <768px) to ensure layout and typography remain usable
- [ ] T038 Verify WCAG AA contrast compliance on key text, badges, and buttons using `/design_assets` as reference and adjust CSS variables in `CustomerPortal.Web/wwwroot/css/app.css` as needed
- [ ] T039 Implement JavaScript interop for mobile sidebar toggle in `CustomerPortal.Web/wwwroot/js/sidebar-toggle.js` and `CustomerPortal.Web/Components/Layout/MainLayout.razor`
- [ ] T040 Perform manual end-to-end testing of dashboard, invoices, invoice detail, payments, and SOA flows following `specs/001-customer-credit-portal/quickstart.md`
- [ ] T041 [P] Add focused xUnit tests for invoice filtering logic and edge cases in `CustomerPortal.Tests/Repositories/InvoiceRepositoryTests.cs`

---

## Phase 8: Enhancements - Payments, Invoices, and Documentation

**Purpose**: Capture additional implemented behaviors and improvements beyond the original baseline (invoice creation, offline payments, SOA PDFs, and docs).

- [x] T042 [P] Add invoice creation DTO and service methods (for example, `CreateInvoiceDto`, `InvoiceService.CreateInvoiceAsync`) and expose a "New Invoice" entry flow in `CustomerPortal.Web/Pages/Invoices.razor` using a modal dialog
- [x] T043 [P] Add payment creation DTO and repository/service methods (for example, `CreatePaymentDto`, `IPaymentRepository.CreatePaymentAsync`, `PaymentService.CreatePaymentAsync`) to record offline payments
- [x] T044 [US3] Implement a "Pay Now"/"Make a Payment" flow on `CustomerPortal.Web/Pages/Dashboard.razor` that opens a payment entry modal, allows the user to enter reference, date, amount, and method, and records an offline payment
- [x] T045 [US4] Implement a "Make a Payment" flow on `CustomerPortal.Web/Pages/Payments.razor` with a modal-based payment entry form that creates a new payment and refreshes the payment history and current statement
- [x] T046 [P] Extend invoice queries and services to surface a list of **open invoices** (for example, `IInvoiceRepository.GetOpenInvoicesAsync`, `InvoiceService.GetOpenInvoicesAsync`) that include at least invoice id, number, status, and total amount
- [x] T047 [P] Wire an "Apply To Invoice" dropdown into the dashboard payment modal (`Dashboard.razor`) so a customer can select an open invoice, auto-fill the payment amount from the invoice total, and optionally default the payment reference to the invoice number; after saving, mark the invoice as paid and refresh dashboard stats/activity
- [x] T048 [P] Wire an "Apply To Invoice" dropdown into the payments page modal (`Payments.razor`) with the same behavior: selecting an invoice auto-fills the amount and, on save, marks the invoice as paid and reloads payments and statement data
- [x] T049 [P] Implement SOA PDF generation using QuestPDF in `CustomerPortal.Web/Services/PaymentService.cs`, producing a tabular statement with Date, Description, Debit, Credit, and running Balance, plus closing balance summary for the current month
- [x] T050 [P] Add a minimal SOA PDF endpoint in `CustomerPortal.Web/Program.cs` (for example, `/soa/pdf?from=...&to=...`) that delegates to `PaymentService` and returns a PDF file result with content type `application/pdf`
- [x] T051 [P] Update `specs/001-customer-credit-portal/spec.md` and `plan.md` to reflect offline payment behavior, invoice-linked payments, SOA PDF generation, and the Make Payment flows on dashboard and payments pages

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies – can start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion – BLOCKS all user stories.
- **User Stories (Phase 3–6)**: Depend on Foundational completion; can proceed sequentially in priority order (P1 → P2 → P3 → P4) or in parallel once shared infrastructure is stable.
- **Polish (Phase 7)**: Depends on all required user stories being complete; can run alongside non-breaking story work.

### User Story Dependencies

- **User Story 1 (P1)**: Depends only on Setup + Foundational; no dependency on other stories.
- **User Story 2 (P2)**: Depends on Setup + Foundational; reuses entities and repositories but is independently testable via `Invoices.razor`.
- **User Story 3 (P3)**: Depends on Setup + Foundational and minimal invoice list plumbing from US2 (to navigate from list to detail), but can be validated independently from URL routes.
- **User Story 4 (P4)**: Depends on Setup + Foundational; independent of invoice detail and can be tested entirely via `Payments.razor`.

### Within Each User Story

- For each story, complete DTOs and repositories before services, then services before UI components/pages.
- Manual tests (and automated tests where defined) should be run before marking the story complete.
- Each story should result in a demonstrable slice of functionality that still works even if later stories are not implemented.

### Parallel Opportunities

- **Setup**: T001–T004 are mostly sequential; avoid [P] here to keep solution scaffolding simple.
- **Foundational**: T006 and T007 can proceed in parallel (different files and responsibilities); T012 can be added in parallel once test project (T011) exists.
- **User Stories**:
  - Within US1, component tasks (T015–T018) can be built in parallel once `DashboardService` contract is stable.
  - Within US2, T021–T022 (DTOs and repository queries) can run in parallel; UI components T024–T025 can also be developed in parallel.
  - Within US3 and US4, DTO/repository tasks can be done in parallel with UI scaffolding.
- Different user stories (US2–US4) can be developed in parallel by separate developers once Foundational is complete and shared contracts are agreed.

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1 (Dashboard).
4. Validate dashboard independently (T020, T040) and demo as MVP.

### Incremental Delivery

1. After MVP, implement User Story 2 (Invoices) and verify independently.
2. Add User Story 3 (Invoice Detail) and verify independently.
3. Add User Story 4 (Payments & SOA) and verify independently.
4. Apply Phase 7 polish tasks across all stories.

### Parallel Team Strategy

- Developer A: Focus on Setup + Foundational, then US1 (Dashboard).
- Developer B: US2 (Invoices) and US3 (Invoice Detail) UI + service wiring.
- Developer C: US4 (Payments & SOA) and cross-cutting polish (WCAG, mobile CSS, JS interop).

Each task above follows the checklist format and includes an ID, optional [P]
marker for parallelizable work, optional [USx] label for story phases, and a
concrete file path to keep the work immediately actionable.
