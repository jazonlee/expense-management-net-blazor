<!--
Sync Impact Report
- Version change: 1.0.0 → 1.1.0
- Modified principles:
	- Design Compliance → Design Compliance (clarified source of truth and workflow)
- Added sections:
	- None
- Removed sections:
	- None
- Templates reviewed:
	- ✅ .specify/templates/plan-template.md (aligned, no changes required)
	- ✅ .specify/templates/spec-template.md (aligned, no changes required)
	- ✅ .specify/templates/tasks-template.md (aligned, no changes required)
	- N/A .specify/templates/commands/* (no command templates present)
- Follow-up TODOs:
	- None; design workflow now explicitly references /design_assets and app.css.
-->

# Expense Management Blazor Constitution

## Core Principles

### Tech Stack & Frameworks

This project enforces a single, opinionated technology stack:

- Application MUST target .NET 9 using the Blazor Server hosting model.
- SQL Server Express MUST be the primary database for all persisted data.
- Data access MUST use Dapper as the only ORM/micro-ORM; Entity Framework Core
	and other ORMs MUST NOT be used.
- Styling MUST use Bootstrap 5 with a mobile-first layout strategy.
- Icons MUST use Bootstrap Icons with the `bi-*` naming convention unless a
	replacement library is explicitly approved through governance.

Rationale: A constrained, consistent stack reduces complexity, improves
onboarding, and keeps performance and diagnostics predictable across the
application.

### Architecture Guidelines

This project follows Clean Architecture with strict separation of concerns:

- The solution MUST be divided into the following logical layers with clear
	responsibilities:
	- `Domain`: Entities and models (POCOs) representing core business concepts.
	- `Application`: Interfaces (for example, `IRepository`), DTOs, and business
		logic.
	- `Infrastructure`: Data access implementations (Dapper SQL queries) and
		database connection factories.
	- `API`: Internal service layer and controllers/endpoints, organized by
		feature folders.
	- `UI`: Blazor components and pages.
- Domain and application layers MUST NOT depend on infrastructure or UI
	concerns.
- All database transactions that span multiple repositories MUST be coordinated
	through a Unit of Work implementation.
- All repositories and services MUST be wired using the built-in .NET
	dependency injection container.

Rationale: Clear boundaries between layers keep the domain model independent,
make testing easier, and allow infrastructure concerns (such as data access)
to evolve without rewriting business logic.

### Coding Standards

This project requires consistent, asynchronous, and secure coding practices:

- C# classes, interfaces, and public methods MUST use PascalCase; local
	variables and parameters MUST use camelCase.
- All I/O operations, including database access and outbound API calls, MUST
	use `async`/`await` with cancellation support where appropriate; synchronous
	blocking I/O is forbidden except in startup/bootstrap code where unavoidable.
- All Dapper queries MUST use parameterized SQL; string concatenation with
	untrusted values is forbidden to prevent SQL injection.
- Stored procedures MAY be used only for complex analytics or validated
	performance hotspots and MUST be documented in the relevant specification and
	reviewed in code review.
- UI components MUST be responsive. The main sidebar MUST collapse into a
	mobile-friendly pattern for viewports narrower than 992px.

Rationale: Consistent naming improves readability, async I/O keeps the server
responsive under load, and parameterized SQL protects against injection
vulnerabilities.

### Design Compliance

This project must faithfully implement the approved design system, using the
HTML and CSS assets as the single source of truth:

- Source of Truth: Engineers MUST strictly follow the HTML structure and CSS
	classes found in the `/design_assets` directory when building UI.
- Workflow: When creating a Blazor page (for example, `Dashboard.razor`),
	engineers MUST first read the corresponding HTML file (for example,
	`/design_assets/dashboard.html`) and port the HTML tags 1:1 into the Razor
	component, adapting only what is necessary for data binding and components.
- WCAG: Color usage and typography MUST maintain WCAG AA contrast ratios as
	demonstrated in the mockups contained in `/design_assets`.
- Theming: Engineers MUST use the CSS variables defined in the design HTML
	files (for example, `--primary-blue`, `--text-muted`) and wire them through
	the global stylesheet (for example, `app.css`); introducing hard-coded colors
	in components is forbidden unless they are added to the design token set.

Rationale: Treating `/design_assets` and the associated CSS variables as the
source of truth ensures pixel-accurate implementation, consistent theming,
and accessible, maintainable UI components.

## Implementation Constraints & Non-Functional Requirements

- The UI MUST remain usable and legible on both desktop and mobile devices,
	with primary flows tested at common breakpoints including mobile widths
	(approximately 320–414px) and laptop/desktop widths.
- New third-party libraries that materially affect architecture, styling, or
	data access (for example, alternative ORMs, CSS frameworks, or UI kits) MUST
	be justified in a specification and approved through governance before use.
- SQL queries used by Dapper MUST be reviewed for performance, including
	avoidance of N+1 patterns and unbounded result sets; pagination MUST be used
	for list views that can grow without bound.
- Accessibility and responsiveness checks MUST be part of the definition of
	done for any feature that changes the UI.

## Development Workflow & Quality Gates

- Every feature specification and implementation plan MUST include a
	"Constitution Check" section explaining how the work complies with this
	constitution or explicitly lists any requested exceptions.
- Pull requests MUST state whether they touch tech stack, architecture,
	styling, or accessibility constraints and MUST confirm compliance with the
	relevant principles above.
- Any exception to a MUST rule requires a documented justification in the
	feature specification or plan and MUST either:
	- Be temporary with a tracked clean-up task, or
	- Be accompanied by a governance proposal to amend this constitution.
- Changes that alter the tech stack, architectural layers, or non-functional
	guarantees MUST update this constitution and bump its version according to
	the governance rules below.

## Governance

- This constitution defines the non-negotiable engineering and design
	constraints for the Expense Management Blazor project and supersedes
	conflicting ad-hoc practices.
- Amendments MUST be proposed via pull request updating
	`.specify/memory/constitution.md` and referencing the intended semantic
	version bump (MAJOR, MINOR, PATCH) with rationale.
- Versioning rules:
	- MAJOR: Any backward-incompatible governance change, removal, or
		redefinition of an existing principle or a relaxation of a MUST constraint.
	- MINOR: Addition of new principles or sections, or material expansion of
		existing guidance that does not invalidate earlier versions.
	- PATCH: Clarifications, wording improvements, and typo fixes that do not
		change intent.
- Reviewers are responsible for verifying that all changes to code, specs,
	and plans are consistent with this constitution or explicitly propose an
	amendment.
- Before merging any pull request that updates this constitution, the Version
	line below MUST be updated and the Sync Impact Report at the top of this
	file MUST be refreshed to describe the change.

**Version**: 1.1.0 | **Ratified**: 2025-12-10 | **Last Amended**: 2025-12-10
