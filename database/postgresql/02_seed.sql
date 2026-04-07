-- MemberCare PostgreSQL seed data
-- Run after 01_schema.sql

BEGIN;

INSERT INTO branches (code, name, city, state_region, country)
VALUES
  ('CENTRAL', 'Central', 'Lagos', 'Lagos', 'Nigeria'),
  ('NORTH', 'North', 'Abuja', 'FCT', 'Nigeria'),
  ('SOUTH', 'South', 'Port Harcourt', 'Rivers', 'Nigeria')
ON CONFLICT (code) DO NOTHING;

INSERT INTO roles (role_code, role_name, description)
VALUES
  ('super_admin', 'Super Administrator', 'Full access to all modules and settings'),
  ('church_admin', 'Church Administrator', 'Operational management across allowed branches'),
  ('pastor', 'Pastor / Minister', 'Pastoral and leadership reports access'),
  ('follow_up', 'Follow-Up Officer', 'Manage visitor, convert, and follow-up records'),
  ('department_leader', 'Department Leader', 'Department membership and attendance'),
  ('fellowship_leader', 'Fellowship / Cell Leader', 'Fellowship records and follow-up visibility'),
  ('attendance', 'Attendance Officer', 'Attendance setup and capture'),
  ('finance', 'Finance Officer', 'Contribution records and reports'),
  ('report_viewer', 'Report Viewer', 'Leadership report read-only access')
ON CONFLICT (role_code) DO NOTHING;

INSERT INTO permissions (permission_code, module_name, action_name, description)
VALUES
  ('dashboard.read', 'Dashboard', 'Read', 'View dashboard widgets'),
  ('members.create', 'Members', 'Create', 'Create member records'),
  ('members.read', 'Members', 'Read', 'View member records'),
  ('members.update', 'Members', 'Update', 'Update member records'),
  ('visitors.create', 'Visitors', 'Create', 'Create visitor records'),
  ('visitors.read', 'Visitors', 'Read', 'View visitor records'),
  ('converts.create', 'New Converts', 'Create', 'Create new convert records'),
  ('converts.read', 'New Converts', 'Read', 'View new convert records'),
  ('attendance.create', 'Attendance', 'Create', 'Create sessions and attendance records'),
  ('attendance.read', 'Attendance', 'Read', 'View attendance records'),
  ('followup.create', 'Follow-Up', 'Create', 'Create follow-up records'),
  ('followup.read', 'Follow-Up', 'Read', 'View follow-up records'),
  ('reports.read', 'Reports', 'Read', 'View reports'),
  ('reports.export', 'Reports', 'Export', 'Export report outputs'),
  ('admin.users.manage', 'Administration', 'Manage Users', 'Manage user accounts'),
  ('admin.roles.manage', 'Administration', 'Manage Roles', 'Manage role permissions')
ON CONFLICT (permission_code) DO NOTHING;

-- Minimal role-permission bootstrapping
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM roles r
JOIN permissions p ON p.permission_code IN (
  'dashboard.read', 'members.create', 'members.read', 'members.update',
  'visitors.create', 'visitors.read', 'converts.create', 'converts.read',
  'attendance.create', 'attendance.read', 'followup.create', 'followup.read',
  'reports.read', 'reports.export', 'admin.users.manage', 'admin.roles.manage'
)
WHERE r.role_code = 'super_admin'
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM roles r
JOIN permissions p ON p.permission_code IN (
  'dashboard.read', 'members.create', 'members.read', 'members.update',
  'visitors.create', 'visitors.read', 'converts.create', 'converts.read',
  'attendance.create', 'attendance.read', 'followup.create', 'followup.read',
  'reports.read', 'reports.export'
)
WHERE r.role_code = 'church_admin'
ON CONFLICT DO NOTHING;

COMMIT;
