# JWT Authentication Guide

## Overview

All MemberCare API endpoints (except `/auth/login`) require JWT Bearer token authentication. Tokens are issued upon successful login and include role and branch claims for authorization.

## Authentication Flow

1. **Login** POST `/auth/login`
   - Request: `{ "username": "admin", "password": "password123" }`
   - Response: `{ "accessToken": "jwt...", "refreshToken": "jwt...", "expiresIn": 3600 }`

2. **Use Token** in subsequent requests
   - Header: `Authorization: Bearer <accessToken>`

3. **Token Expiry**
   - Tokens expire after `Jwt:ExpirationMinutes` (default: 60 minutes)
   - Refresh tokens can be used to obtain new access tokens (not yet implemented)

## JWT Token Claims

Each JWT contains:

| Claim | Value | Example |
| --- | --- | --- |
| `sub` (NameIdentifier) | Username | "admin" |
| `name` | Username | "admin" |
| `role` | User role | "super_admin" \| "church_admin" \| "pastor" \| ... |
| `branch_id` | Assigned branch UUID | "550e8400-e29b-41d4-a716-446655440000" |
| `exp` | Token expiration | Unix timestamp |
| `iss` | Issuer | "membercare-api" |
| `aud` | Audience | "membercare-client" |

## Roles and Permissions

The following roles are recognized:

| Role | Description | Use Case |
| --- | --- | --- |
| `super_admin` | Full system access | System administrator |
| `church_admin` | Organization-level access | Church leadership |
| `pastor` | Ministry oversight | Pastoral staff |
| `follow_up_officer` | Visitor follow-up | Discipleship/outreach |
| `attendance_officer` | Attendance management | Attendance tracking |
| `finance_officer` | Financial reporting | Finance team |
| `report_viewer` | Read-only reporting | Reporting only |

## Authorization Policies

Endpoints are protected by named authorization policies:

| Policy | Allowed Roles |
| --- | --- |
| `MemberManagement` | super_admin, church_admin |
| `VisitorManagement` | super_admin, church_admin, pastor, follow_up_officer |
| `AttendanceManagement` | super_admin, church_admin, attendance_officer |
| `FollowUpManagement` | super_admin, church_admin, pastor, follow_up_officer |
| `Reports` | super_admin, church_admin, pastor, finance_officer, report_viewer |
| `ChurchAdmin` | super_admin, church_admin |

## Demo Credentials

For development/testing, the API automatically assigns roles based on username:

| Username | Assigned Role |
| --- | --- |
| `admin` | super_admin |
| `pastor` | pastor |
| `visitorcare` | follow_up_officer |
| `attendance` | attendance_officer |
| `finance` | finance_officer |
| `reports` | report_viewer |
| (any other) | church_admin (default) |

**Example**: `POST /auth/login` with `{ "username": "pastor", "password": "anything" }` returns a token with role claim `"pastor"`.

## Configuration

JWT settings are configured in `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-change-in-production-at-least-32-chars-for-256bit",
    "Issuer": "membercare-api",
    "Audience": "membercare-client",
    "ExpirationMinutes": 60
  }
}
```

**IMPORTANT**: Change `SecretKey` in production to a unique, randomly generated value (minimum 32 characters for HS256).

## Error Responses

| Status | Scenario |
| --- | --- |
| 200 | Authentication successful |
| 400 | Missing/invalid credentials |
| 401 | Invalid token or token expired |
| 403 | User role not authorized for endpoint |

Example:
```
HTTP/1.1 403 Forbidden
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "The user is not authorized to access this resource."
}
```

## Testing with cURL / Postman

### Get Token
```bash
curl -X POST http://localhost:8080/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"test123"}'
```

### Use Token
```bash
curl -X GET http://localhost:8080/v1/members \
  -H "Authorization: Bearer eyJhbGc..."
```

## Next Steps

- [ ] Integrate with database user management (replace demo role assignment)
- [ ] Implement refresh token rotation
- [ ] Add multi-tenancy row-level security (branch_id enforcement)
- [ ] Enable HTTPS/TLS in production
- [ ] Implement audit logging for auth events
