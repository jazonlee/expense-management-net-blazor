# Data Model: Customer Credit Portal

**Feature**: 001-customer-credit-portal  
**Date**: 2025-12-10

This document describes the core entities, fields, relationships, and validation
rules for the Customer Credit Portal feature. It is implementation-agnostic but
assumes a relational schema in SQL Server Express and C# POCOs in
`CustomerPortal.Core`.

---

## Overview

Core business entities:

- `Customer`
- `Invoice`
- `InvoiceItem`
- `Payment`
- `Activity`
- `Notification`

The portal is scoped to a single customer per authenticated user; all queries
are implicitly filtered by `CustomerId`.

---

## Entity: Customer

Represents a customer account whose credit status, invoices, and payments are
shown in the portal.

- **CustomerId** (PK, `uniqueidentifier` or `int`)
- **CustomerNumber** (`nvarchar(50)`, unique) – human-facing identifier.
- **Name** (`nvarchar(200)`, required)
- **BillingAddressLine1** (`nvarchar(200)`, required)
- **BillingAddressLine2** (`nvarchar(200)`, nullable)
- **BillingCity** (`nvarchar(100)`, required)
- **BillingState** (`nvarchar(100)`, nullable)
- **BillingPostalCode** (`nvarchar(20)`, required)
- **BillingCountry** (`nvarchar(100)`, required)
- **ShippingAddressLine1** (`nvarchar(200)`, required)
- **ShippingAddressLine2** (`nvarchar(200)`, nullable)
- **ShippingCity** (`nvarchar(100)`, required)
- **ShippingState** (`nvarchar(100)`, nullable)
- **ShippingPostalCode** (`nvarchar(20)`, required)
- **ShippingCountry** (`nvarchar(100)`, required)
- **CreditLimit** (`decimal(18,2)`, required)
- **AvailableCredit** (`decimal(18,2)`, computed or derived)
- **CurrentBalance** (`decimal(18,2)`, computed or derived)

**Relationships**:

- One `Customer` to many `Invoice`.
- One `Customer` to many `Payment`.

**Validation / Rules**:

- `CreditLimit` MUST be non-negative.
- `AvailableCredit` = `CreditLimit` − (sum of outstanding invoice balances).
- `CurrentBalance` should reflect total outstanding amount on unpaid/overdue
  invoices.

---

## Entity: Invoice

Represents a billing document issued to a `Customer`.

- **Id** (PK, `uniqueidentifier` or `int`)
- **CustomerId** (FK → `Customer.CustomerId`, required)
- **InvoiceNumber** (`nvarchar(50)`, required, unique per customer)
- **IssuedDate** (`datetime2`, required)
- **DueDate** (`datetime2`, required)
- **Status** (`nvarchar(20)`, required) – enum-like values: `Pending`, `Paid`,
  `Overdue`.
- **SubtotalAmount** (`decimal(18,2)`, required)
- **DiscountAmount** (`decimal(18,2)`, required, default 0)
- **TaxAmount** (`decimal(18,2)`, required)
- **TotalAmount** (`decimal(18,2)`, required)

**Relationships**:

- One `Invoice` to many `InvoiceItem`.
- One `Customer` to many `Invoice`.

**Validation / Rules**:

- `IssuedDate` MUST be on or before `DueDate`.
- `SubtotalAmount` MUST equal sum of `InvoiceItem.Quantity * InvoiceItem.UnitPrice`.
- `TaxAmount` SHOULD be `round(0.08 * (SubtotalAmount - DiscountAmount), 2)`
  where applicable.
- `TotalAmount` MUST equal `SubtotalAmount - DiscountAmount + TaxAmount`.
- `Status`:
  - `Paid` only if associated payments fully cover `TotalAmount`.
  - `Overdue` if `DueDate` < today and status is not `Paid`.

---

## Entity: InvoiceItem

Represents an individual line item on an `Invoice`.

- **Id** (PK, `uniqueidentifier` or `int`)
- **InvoiceId** (FK → `Invoice.Id`, required)
- **Description** (`nvarchar(200)`, required)
- **Quantity** (`decimal(18,2)`, required, > 0)
- **UnitPrice** (`decimal(18,4)`, required, ≥ 0)
- **LineTotal** (`decimal(18,2)`, required)

**Validation / Rules**:

- `LineTotal` MUST equal `Quantity * UnitPrice` rounded to 2 decimals.
- `Quantity` MUST be positive.
- `UnitPrice` MUST be non-negative.

---

## Entity: Payment

Represents a payment made by a `Customer`.

- **Id** (PK, `uniqueidentifier` or `int`)
- **CustomerId** (FK → `Customer.CustomerId`, required)
- **PaymentRef** (`nvarchar(100)`, required) – external or human-facing
  reference.
- **Date** (`datetime2`, required)
- **Amount** (`decimal(18,2)`, required, > 0)
- **Method** (`nvarchar(50)`, required) – e.g., `CreditCard`, `BankTransfer`.
- **ReceiptUrl** (`nvarchar(500)`, nullable)

**Relationships**:

- One `Customer` to many `Payment`.
- Association to `Invoice`(s) may be via a join table or external
  reconciliation process (not enforced here but relevant to reporting).

**Validation / Rules**:

- `Amount` MUST be positive.
- `Method` MUST be one of an allowed set (for example, `CreditCard`,
  `BankTransfer`, extendable via configuration).

---

## Entity: Activity

Represents a timeline event shown on the dashboard.

- **Id** (PK, `uniqueidentifier` or `int`)
- **CustomerId** (FK → `Customer.CustomerId`, required)
- **Title** (`nvarchar(200)`, required)
- **Date** (`datetime2`, required)
- **AmountChange** (`decimal(18,2)`, required, may be 0)
- **Status** (`nvarchar(50)`, required) – e.g., `Created`, `Paid`, `Overdue`.
- **ActivityType** (`nvarchar(50)`, required) – e.g., `Invoice`, `Payment`.
- **ReferenceId** (`uniqueidentifier` or `int`, nullable) – links back to
  `Invoice` or `Payment` when applicable.

**Validation / Rules**:

- `ActivityType` and `ReferenceId` should be consistent (for example, if
  `ActivityType = 'Invoice'`, `ReferenceId` should reference an `Invoice`).
- `AmountChange` may be positive (charges) or negative (payments/credits) or 0
  for informational events.

---

## Entity: Notification

Represents a notice card shown on the dashboard.

- **Id** (PK, `uniqueidentifier` or `int`)
- **Title** (`nvarchar(200)`, required)
- **Message** (`nvarchar(max)`, required)
- **Type** (`nvarchar(50)`, required) – e.g., `Maintenance`, `Info`, `Warning`.
- **StartDate** (`datetime2`, required)
- **EndDate** (`datetime2`, nullable) – when null, treated as open-ended.
- **IsActive** (`bit`, computed or derived) – true when `StartDate <= now` and
  (`EndDate` is null or `EndDate >= now`).

**Validation / Rules**:

- `EndDate` (if present) MUST be on or after `StartDate`.
- Only notifications with `IsActive = true` should be shown on the dashboard.

---

## Derived Views

To support dashboard stats, invoice lists, and SOA generation, the
implementation MAY introduce SQL views or query models such as:

- **DashboardSummaryView** – aggregates credit limit, available credit, and due
  balance per `CustomerId`.
- **InvoiceListView** – flattens invoice and customer data needed for the
  invoices table and filters.
- **PaymentHistoryView** – exposes payment records with computed fields useful
  for the Payments & SOA page.
- **StatementOfAccountView** – summarizes opening balance, movements, and
  closing balance for a calendar month per customer.

These views do not change the logical entity model but can simplify Dapper
queries and keep Blazor components focused on presentation.
