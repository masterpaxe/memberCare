# MemberCare Backend Scaffold

This folder contains the ASP.NET Core Web API scaffold for the MemberCare SRS Phase 1 scope.

## Project Layout

- MemberCare.slnx at repository root
- backend/MemberCare.Api as the API project
- API contracts in ../api/contracts
- Database scripts in ../database/postgresql

## Implemented Endpoints (v1)

- POST /v1/auth/login
- POST /v1/auth/refresh
- GET /v1/dashboard/summary
- GET/POST/PATCH/DELETE /v1/members
- GET/POST /v1/visitors
- POST /v1/visitors/{visitorId}/convert
- GET/POST /v1/new-converts
- GET/POST /v1/attendance/sessions
- POST /v1/attendance/records
- GET/POST /v1/follow-up/records
- GET /v1/reports/summary
- GET /v1/admin/users

Note: Data layer is currently in-memory for rapid scaffolding and contract verification.

## Run

```bash
dotnet restore MemberCare.slnx
dotnet build MemberCare.slnx
dotnet run --project backend/MemberCare.Api/MemberCare.Api.csproj
```

OpenAPI document during development:

- /openapi/v1.json

## Next Hardening Steps

1. Replace in-memory services with PostgreSQL repositories.
2. Add JWT auth and claims-based policy enforcement from the RBAC matrix.
3. Add branch scope middleware and row-level restrictions.
4. Add FluentValidation for requests and standardized ProblemDetails responses.
5. Add unit/integration tests for each controller.
