# MemberCare API Contracts

This folder defines implementation-ready backend API contracts for the SRS Phase 1 scope.

## Files

- contracts/openapi.yaml: OpenAPI 3.1 specification
- contracts/rbac-matrix.md: Role-to-endpoint authorization map

## Suggested Backend Implementation Sequence

1. Create ASP.NET Core Web API project with versioned base path /v1.
2. Implement JWT authentication endpoints under /auth.
3. Implement branch scoping middleware from authenticated user claims.
4. Implement Members, Visitors, New Converts APIs.
5. Implement Attendance Sessions and Records APIs.
6. Implement Follow-Up Records APIs.
7. Implement Dashboard and Reports read endpoints.
8. Add audit log write behavior to mutating operations.

## Recommended Claim Set

- sub: user id
- role: one or more role codes
- branch_id: user branch scope
- perms: optional fine-grained permissions list

## Contract Governance

- Keep openapi.yaml as source of truth.
- Any breaking change requires version bump.
- Add contract tests that validate request/response shape against spec.
