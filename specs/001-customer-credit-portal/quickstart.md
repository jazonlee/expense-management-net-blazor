# Quickstart: Customer Credit Portal Feature

**Feature**: 001-customer-credit-portal  
**Last Updated**: 2025-12-10

This quickstart explains how to set up the solution structure for the Customer
Credit Portal feature and how to manually exercise the key user flows once the
feature is implemented.

---

## 1. Solution & Projects

Planned structure (see `plan.md` for full details):

- `CustomerPortal.sln`
  - `CustomerPortal.Core` (class library)
    - `Domain/` – entities: Customer, Invoice, InvoiceItem, Payment, Activity,
      Notification.
    - `Application/` – DTOs, interfaces (`IInvoiceRepository`,
      `IPaymentRepository`, `IUnitOfWork`), and optional application services.
  - `CustomerPortal.Web` (Blazor Server app)
    - `Infrastructure/` – Dapper repositories, SQL connection factory,
      Unit-of-Work, migrations and seed scripts.
    - `Services/` – `InvoiceService`, `PaymentService`, `DashboardService`
      consuming repositories.
    - `Pages/` – `Dashboard.razor`, `Invoices.razor`, `InvoiceDetail.razor`,
      `Payments.razor`.
    - `Components/` – shared UI components (e.g., `StatsCard`, `ActivityList`,
      invoice filter and table components).
    - `wwwroot/css/app.css` – CSS variables and Bootstrap-based styles ported
      from `/design_assets`.

---

## 2. Create the Solution Skeleton

From the repository root (`expense-management-net-blazor`), run the following
from a terminal (PowerShell example):

```powershell
# 1. Create solution and projects (if they do not already exist)
dotnet new sln -n CustomerPortal

dotnet new classlib -n CustomerPortal.Core

dotnet new blazorserver -n CustomerPortal.Web

# 2. Add projects to solution
dotnet sln CustomerPortal.sln add .\CustomerPortal.Core\CustomerPortal.Core.csproj

dotnet sln CustomerPortal.sln add .\CustomerPortal.Web\CustomerPortal.Web.csproj

# 3. Add project reference from Web to Core
cd .\CustomerPortal.Web

dotnet add reference ..\CustomerPortal.Core\CustomerPortal.Core.csproj
```

> Note: Adjust commands if the solution or projects already exist.

---

## 3. Wire Up Dependencies

In `CustomerPortal.Web`:

1. Add NuGet packages:

   ```powershell
   dotnet add package Dapper
   dotnet add package Microsoft.Data.SqlClient
  dotnet add package Swashbuckle.AspNetCore # optional if exposing REST API
  dotnet add package QuestPDF # used for SOA PDF generation
   ```

2. Configure DI in `Program.cs`:
   - Register a SQL connection factory using the configured connection string.
   - Register `IInvoiceRepository`, `IPaymentRepository`, and `IUnitOfWork`
     with their Dapper-based implementations.
   - Register `InvoiceService`, `PaymentService`, and `DashboardService`.

3. Configure routing/endpoints:
   - Map Blazor Server hub and fallback page as usual.
   - Optionally map minimal APIs or controllers that implement the contracts in
     `contracts/openapi-customer-credit-portal.yaml`.

---

## 4. Database Setup

1. Create a SQL Server Express database (for example, `CustomerPortalDb`).
2. Apply schema for tables:
   - `Customers`
   - `Invoices`
   - `InvoiceItems`
   - `Payments`
3. Seed mock data representing at least:
   - Several invoices (Pending, Paid, Overdue) for a single test customer.
   - Matching payments.
   - Activities and notifications consistent with the dashboard mockups.

You can maintain the schema as SQL scripts under
`CustomerPortal.Web/Infrastructure/Migrations/`.

---

## 5. Port Design Assets

1. From `/design_assets`, locate the HTML files for:
   - Dashboard
   - Invoices list
   - Invoice detail
   - Payments/SOA page

2. For each page:
   - Create a corresponding `.razor` file in `CustomerPortal.Web/Pages`.
   - Copy the HTML structure and Bootstrap classes 1:1 into the Razor file.
   - Replace static values with `@` bindings to DTOs returned by the services.

3. Move CSS variables and shared styles from the design HTML into
   `CustomerPortal.Web/wwwroot/css/app.css`, ensuring variables like
   `--primary-blue` and `--text-muted` are preserved.

---

## 6. Manual Test Flows

Once wired, you should be able to:

1. **Dashboard**
   - Navigate to `/` or `/dashboard`.
   - Verify Available Credit, Credit Limit, and Due Balance match seeded data.
   - Confirm recent activity entries correspond to invoice and payment events.
   - Confirm any active notifications appear as cards.

2. **Invoices**
   - Navigate to `/invoices`.
   - Use filters for invoice number, status, and amount range; verify results
     and pagination.
   - Open an invoice detail from the Actions column.
   - Use the **New Invoice** entry flow (for example, the "New Invoice" button
     and modal) to create an invoice and confirm it appears in the invoices
     list and influences dashboard stats and recent activity.

3. **Invoice Detail**
   - On `/invoices/{id}`, ensure header, Bill To, Ship To, line items, and
     summary (Subtotal, Discount, Tax @ 8%, Total) are correct.
   - Use "Print" and "Download PDF" actions (stubbed or wired to real
     endpoints as available).
   - Use "Pay Now" and confirm the invoice transitions to a paid/offline state
     per the design.

4. **Payments & SOA**
   - Navigate to `/payments`.
   - Filter by date range and method; verify table contents.
   - Click "Download SOA (PDF)" and confirm a statement for the current
     calendar month is returned as a real PDF showing invoices, payments,
     and a closing balance for the period.
   - Click "Make a Payment" to open the payment entry modal. Enter a
     payment reference, date, amount, and method, then submit and confirm
     the new payment appears in the payment history and updates the
     statement card.
   - In the payment entry modal, choose an invoice from the "Apply To
     Invoice" dropdown and confirm that the payment amount defaults to the
     selected invoice's total and the invoice no longer appears as open
     after the payment is saved.

---

## 7. Next Steps

- Use `spec.md`, `data-model.md`, and `contracts/openapi-customer-credit-portal.yaml`
  as the authoritative guides during implementation.
- When ready to break down work, generate `tasks.md` via `/speckit.tasks` based
  on this plan and the specification.
