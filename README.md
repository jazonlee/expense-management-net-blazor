# Expense Management / Customer Credit Portal

This repository contains a .NET 9 Blazor Server implementation of a Customer Credit Portal. It includes a dashboard, invoices list and detail, payments history, and Statement of Account (SOA) PDF generation.
Code Generation & Workflow (Speckit + Copilot)

This entire codebase was generated and evolved using the Speckit tooling and GitHub Copilot inside VS Code, following a spec‑driven workflow rather than ad‑hoc coding.

At a high level:

**1. Specification first (spec.md)**
The feature started from a human‑readable specification in spec.md, describing user stories, functional requirements, success criteria, and edge cases for the Customer Credit Portal (dashboard, invoices, invoice detail, payments, and SOA).

**2. Planning (/speckit.plan)**
Speckit generated an implementation plan in plan.md, outlining the tech stack (Blazor Server, Dapper, SQL Server Express), project structure, and layering between CustomerPortal.Core, CustomerPortal.Web, and tests.

**3. Task breakdown (tasks.md)**
Using /speckit.tasks, the plan was turned into a concrete, ordered task list in tasks.md (T001–T0xx), grouped by phases and user stories. Each task references specific files and is tracked with checklist syntax, so you can see exactly which pieces were implemented.

**4. Guided implementation**
Copilot then implemented the solution by executing those tasks, phase by phase:

- Creating the solution/projects and folder structure.
- Defining domain entities and DTOs.
- Implementing Dapper repositories and UnitOfWork.
- Building Blazor pages/components by porting HTML from design_assets.
- Adding services (Dashboard, Invoice, Payment) and the QuestPDF‑based SOA generator.
- Implementing enhancements like New Invoice, Make Payment modals, invoice selection in payment flows, and real SOA PDFs.

**5. Continuous alignment**
As the code evolved, the Speckit artifacts were kept in sync:

- spec.md was updated to reflect offline payments, invoice‑linked payments, and SOA behavior.
- tasks.md gained a Phase 8 to track and mark off enhancements like the New Invoice modal and payment entry screens.
- quickstart.md was refreshed to describe how to run and manually test the implemented flows.

Because of this workflow, the repository is traceable: you can go from a requirement in spec.md → to a task in tasks.md → to the corresponding code in CustomerPortal.Core, CustomerPortal.Web, or CustomerPortal.Tests, and back again.

## Solution Structure

- `CustomerPortal.sln` – root solution
- `CustomerPortal.Core/` – domain entities and application layer (DTOs, interfaces)
- `CustomerPortal.Web/` – Blazor Server app (UI, services, Dapper repositories, SOA endpoint)
- `CustomerPortal.Tests/` – xUnit tests for repositories and services
- `specs/001-customer-credit-portal/` – feature Speckit tooling's specification, plan, tasks, and quickstart
- `.specify\memory\` - feature Speckit tooling's constitution file

## Prerequisites

- .NET 9 SDK
- SQL Server Express (or compatible SQL Server instance)

## Running the App

From the repository root:

```powershell
cd "C:\Users\<your repo>\expense-management-net-blazor"
cd .\CustomerPortal.Web
 dotnet run
```

Then open the URL shown in the console (for example, `http://localhost:5044`).

## Tests

To run the automated tests:

```powershell
cd "C:\Users\<your repo>\expense-management-net-blazor"
dotnet test
```

## GitHub

This repo is ready to be pushed to GitHub. After creating a repository on GitHub, you can connect this local repo and push:

```powershell
cd "C:\Users\<your repo>\expense-management-net-blazor"
# If not already a git repo, initialize and commit
# git init
# git add .
# git commit -m "Initial commit"

# Add your GitHub remote (replace <your-username> and repo name as needed)
git remote add origin https://github.com/<your-username>/expense-management-net-blazor.git
git branch -M main
git push -u origin main
```
