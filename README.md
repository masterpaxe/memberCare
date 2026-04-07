# MemberCare

Church Management and Member Care System MVP built from the provided SRS (v1.0, April 6, 2026).

## What This Repository Contains

This repository currently implements a browser-based Phase 1 prototype using plain HTML, CSS, and JavaScript with localStorage persistence.

Implemented modules:

- Dashboard with key operational indicators and alerts
- Member Management
- Visitor / First-Timer Management
- New Convert Management
- Attendance Sessions and Attendance Records
- Follow-Up and Pastoral Care records (basic)
- Reports snapshot and JSON export
- Basic branch-based visibility simulation

## Run Locally

1. Start the backend API:
  - `dotnet run --project backend/MemberCare.Api/MemberCare.Api.csproj --urls http://localhost:8080`
2. Open `index.html` in any modern browser.
3. Use the top-right session card and click `Apply` to authenticate as the selected role.
4. Navigate modules from the left menu.
5. Enter records using each module form.

The frontend now reads/writes live data through the ASP.NET API (PostgreSQL-backed).

## SRS to MVP Coverage

Phase 1 items from the SRS covered in this prototype:

- Authentication and role management (simulated by role selector)
- Dashboard
- Member management
- Visitor management
- New convert management
- Attendance management
- Follow-up records
- Basic reports

Not yet implemented in this repository:

- Full frontend token/session UX (auto-refresh, sign-out, role banners)
- Multi-user concurrency and central database
- Department and fellowship full CRUD lifecycle
- Communication (SMS/email) integrations
- Full branch administration and consolidated leadership views
- Audit logging and advanced analytics
- Contributions, events, classes, and notifications modules

## Recommended Next Build Step

Move from prototype to production architecture:

- Frontend: React with module-based routing
- Backend: ASP.NET Core Web API
- Database: SQL Server or PostgreSQL
- Identity: ASP.NET Core Identity with role-based policies
- Reporting: API-backed filtered exports
- Integrations: SMS/email service providers

## Suggested Delivery Plan

1. Define domain model and relational schema from SRS core entities.
2. Build secure auth and role permissions first.
3. Implement Members, Visitors, Converts, Attendance APIs and UI.
4. Add Follow-Up workflows and report endpoints.
5. Add audit logs and branch restrictions.
6. Expand with Phase 2 and Phase 3 modules.

## Database Starter Pack (Added)

A production-oriented PostgreSQL schema package is now included under `database/postgresql`:

- `01_schema.sql` for tables, constraints, and indexes
- `02_seed.sql` for branches, roles, permissions, and baseline mappings
- `03_views.sql` for reporting views
- `04_sample_queries.sql` for common analytics/reporting queries

Quick run order:

1. `database/postgresql/01_schema.sql`
2. `database/postgresql/02_seed.sql`
3. `database/postgresql/03_views.sql`

See `database/postgresql/README.md` for execution and design notes.

## API Contract Pack (Added)

Implementation-ready API artifacts are now included under `api`:

- `api/contracts/openapi.yaml` for OpenAPI 3.1 endpoint contracts
- `api/contracts/rbac-matrix.md` for role-to-endpoint access rules
- `api/contracts/http-scenarios.http` for manual endpoint smoke tests
- `api/README.md` for backend implementation guidance

These contracts align to the Phase 1 SRS modules:

- Auth and role-aware access
- Dashboard summary
- Members
- Visitors and conversion to member
- New converts
- Attendance sessions and records
- Follow-up records
- Reports summary

## Backend Scaffold (Added)

An ASP.NET Core Web API scaffold is now included:

- Solution: `MemberCare.slnx`
- API project: `backend/MemberCare.Api`
- Backend notes: `backend/README.md`

Quick commands:

1. `dotnet restore MemberCare.slnx`
2. `dotnet build MemberCare.slnx`
3. `dotnet run --project backend/MemberCare.Api/MemberCare.Api.csproj`

Current backend status:

- Versioned `v1` routes matching the API contracts
- PostgreSQL-backed service layer using Npgsql plus Dapper
- **JWT Bearer authentication** with role-based authorization policies
  - Token generation in `/auth/login` endpoint
  - Claims include user role and assigned branch_id
  - Protected endpoints enforce role-based access control
- **Branch-scoped multi-tenant data isolation**
  - Users can only access data in their assigned branch
  - Super admins have unrestricted access across all branches
  - Row-level enforcement on all queries
  - 404 for read access outside branch, 403 for write access
- Ready for input validation, test automation, and production hardening

For auth setup and testing, see `api/contracts/AUTH-GUIDE.md`
