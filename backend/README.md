# MemberCare Backend Scaffold

This folder contains the ASP.NET Core Web API scaffold for the MemberCare SRS Phase 1 scope.

## Project Layout

- MemberCare.slnx at repository root
- backend/MemberCare.Api as the API project
- API contracts in ../api/contracts
- Database scripts in ../database/postgresql

## Implemented Endpoints (v1)

- GET /v1/health
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

Data layer is now PostgreSQL-backed through Npgsql plus Dapper, using the schema in database/postgresql.

## Run

```bash
dotnet restore MemberCare.slnx
dotnet build MemberCare.slnx
dotnet run --project backend/MemberCare.Api/MemberCare.Api.csproj
```

OpenAPI document during development:

- /openapi/v1.json

Smoke test command:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/smoke-test.ps1
```

## Next Hardening Steps

1. ✅ JWT auth and claims-based policy enforcement (COMPLETED)
   - JWT token generation with role and branch claims
   - Bearer token validation middleware  
   - Role-based authorization policies on protected endpoints
   - See ../api/contracts/AUTH-GUIDE.md for setup and testing

2. ✅ Branch scope middleware and row-level restrictions (COMPLETED)
   - BranchContext service extracts branch_id from JWT claims
   - All data queries filtered by user's assigned branch_id
   - Super admins (role=super_admin) can access all branches
   - Read operations return 404 if record outside user's branch (don't leak existence)
   - Write operations return 403 Forbidden if outside user's branch
   - ExceptionHandlingMiddleware converts violations to proper HTTP responses

3. Add FluentValidation for requests and standardized ProblemDetails responses.

4. Add unit/integration tests for each controller and service.

5. Add migration scripts.

## Authentication & Authorization

All endpoints except `POST /v1/auth/login` require a valid JWT Bearer token.

Test token generation:
```bash
curl -X POST http://localhost:8080/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"anypassword"}'
```

Response includes `accessToken` for use in subsequent requests:
```
Authorization: Bearer <accessToken>
```

For detailed auth configuration and testing, see ../api/contracts/AUTH-GUIDE.md

## Branch-Scoped Data Access

MemberCare enforces multi-tenant isolation at the row level:

- **Super Admin users** (role = "super_admin"): Unrestricted access across all branches
- **Regular users**: Can only see/modify data in their assigned branch

The branch_id is extracted from JWT claims and automatically enforced on all queries.
When a user without access attempts to:
- **Read** a record outside their branch → 404 Not Found
- **Modify** a record outside their branch → 403 Forbidden
- **Create** in another branch → 403 Forbidden with message "Cannot create members outside your assigned branch"

See BranchContext.cs and the service layer implementation for details.
