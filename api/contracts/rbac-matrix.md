# MemberCare RBAC Endpoint Matrix

This matrix maps API routes to the minimum required role capability.

Legend:
- Allow: Role can invoke endpoint
- Deny: Role cannot invoke endpoint

| Endpoint | Super Admin | Church Admin | Pastor | Follow-Up Officer | Attendance Officer | Finance Officer | Report Viewer |
| --- | --- | --- | --- | --- | --- | --- | --- |
| POST /auth/login | Allow | Allow | Allow | Allow | Allow | Allow | Allow |
| GET /dashboard/summary | Allow | Allow | Allow | Allow | Allow | Allow | Allow |
| GET /members | Allow | Allow | Allow | Allow | Deny | Deny | Allow |
| POST /members | Allow | Allow | Deny | Deny | Deny | Deny | Deny |
| PATCH /members/{memberId} | Allow | Allow | Deny | Deny | Deny | Deny | Deny |
| GET /visitors | Allow | Allow | Allow | Allow | Deny | Deny | Allow |
| POST /visitors | Allow | Allow | Deny | Allow | Deny | Deny | Deny |
| POST /visitors/{visitorId}/convert | Allow | Allow | Deny | Allow | Deny | Deny | Deny |
| GET /new-converts | Allow | Allow | Allow | Allow | Deny | Deny | Allow |
| POST /new-converts | Allow | Allow | Deny | Allow | Deny | Deny | Deny |
| GET /attendance/sessions | Allow | Allow | Allow | Deny | Allow | Deny | Allow |
| POST /attendance/sessions | Allow | Allow | Deny | Deny | Allow | Deny | Deny |
| POST /attendance/records | Allow | Allow | Deny | Deny | Allow | Deny | Deny |
| GET /follow-up/records | Allow | Allow | Allow | Allow | Deny | Deny | Allow |
| POST /follow-up/records | Allow | Allow | Deny | Allow | Deny | Deny | Deny |
| GET /reports/summary | Allow | Allow | Allow | Deny | Deny | Allow | Allow |
| GET /admin/users | Allow | Allow | Deny | Deny | Deny | Deny | Deny |

## Branch Scope Rule

All non-super-admin users must be constrained to their assigned branch_id unless granted explicit multi-branch permission.

## Sensitive Data Rule

Records marked as Sensitive or Strict in pastoral care should require dedicated permissions and should not be exposed to report_viewer.
