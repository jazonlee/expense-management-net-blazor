# Expense Management / Customer Credit Portal

This repository contains a .NET 9 Blazor Server implementation of a Customer Credit Portal. It includes a dashboard, invoices list and detail, payments history, and Statement of Account (SOA) PDF generation.

## Solution Structure

- `CustomerPortal.sln` – root solution
- `CustomerPortal.Core/` – domain entities and application layer (DTOs, interfaces)
- `CustomerPortal.Web/` – Blazor Server app (UI, services, Dapper repositories, SOA endpoint)
- `CustomerPortal.Tests/` – xUnit tests for repositories and services
- `specs/001-customer-credit-portal/` – feature specification, plan, tasks, and quickstart

## Prerequisites

- .NET 9 SDK
- SQL Server Express (or compatible SQL Server instance)

## Running the App

From the repository root:

```powershell
cd "C:\Users\Jason Lee\source\repos\expense-management-net-blazor"
cd .\CustomerPortal.Web
 dotnet run
```

Then open the URL shown in the console (for example, `http://localhost:5044`).

## Tests

To run the automated tests:

```powershell
cd "C:\Users\Jason Lee\source\repos\expense-management-net-blazor"
dotnet test
```

## GitHub

This repo is ready to be pushed to GitHub. After creating a repository on GitHub, you can connect this local repo and push:

```powershell
cd "C:\Users\Jason Lee\source\repos\expense-management-net-blazor"
# If not already a git repo, initialize and commit
# git init
# git add .
# git commit -m "Initial commit"

# Add your GitHub remote (replace <your-username> and repo name as needed)
git remote add origin https://github.com/<your-username>/expense-management-net-blazor.git
git branch -M main
git push -u origin main
```
