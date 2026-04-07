-- MemberCare PostgreSQL Schema (SRS-aligned)
-- Version: 1.0.0
-- Created: 2026-04-07

BEGIN;

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS branches (
  branch_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  code VARCHAR(32) NOT NULL UNIQUE,
  name VARCHAR(120) NOT NULL UNIQUE,
  address_line1 VARCHAR(180),
  city VARCHAR(80),
  state_region VARCHAR(80),
  country VARCHAR(80),
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS roles (
  role_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  role_code VARCHAR(64) NOT NULL UNIQUE,
  role_name VARCHAR(120) NOT NULL,
  description TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS permissions (
  permission_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  permission_code VARCHAR(128) NOT NULL UNIQUE,
  module_name VARCHAR(80) NOT NULL,
  action_name VARCHAR(80) NOT NULL,
  description TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS role_permissions (
  role_id UUID NOT NULL REFERENCES roles(role_id) ON DELETE CASCADE,
  permission_id UUID NOT NULL REFERENCES permissions(permission_id) ON DELETE CASCADE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  PRIMARY KEY (role_id, permission_id)
);

CREATE TABLE IF NOT EXISTS users (
  user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID REFERENCES branches(branch_id) ON DELETE SET NULL,
  username VARCHAR(80) NOT NULL UNIQUE,
  email VARCHAR(180) NOT NULL UNIQUE,
  password_hash TEXT NOT NULL,
  first_name VARCHAR(80) NOT NULL,
  last_name VARCHAR(80) NOT NULL,
  phone VARCHAR(30),
  status VARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (status IN ('Active', 'Inactive', 'Locked')),
  last_login_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS user_roles (
  user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
  role_id UUID NOT NULL REFERENCES roles(role_id) ON DELETE CASCADE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  PRIMARY KEY (user_id, role_id)
);

CREATE TABLE IF NOT EXISTS departments (
  department_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID NOT NULL REFERENCES branches(branch_id) ON DELETE RESTRICT,
  department_name VARCHAR(120) NOT NULL,
  description TEXT,
  leader_member_id UUID,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(branch_id, department_name)
);

CREATE TABLE IF NOT EXISTS fellowships (
  fellowship_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID NOT NULL REFERENCES branches(branch_id) ON DELETE RESTRICT,
  fellowship_name VARCHAR(120) NOT NULL,
  host_name VARCHAR(120),
  meeting_day VARCHAR(32),
  leader_member_id UUID,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(branch_id, fellowship_name)
);

CREATE TABLE IF NOT EXISTS members (
  member_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID NOT NULL REFERENCES branches(branch_id) ON DELETE RESTRICT,
  department_id UUID REFERENCES departments(department_id) ON DELETE SET NULL,
  fellowship_id UUID REFERENCES fellowships(fellowship_id) ON DELETE SET NULL,
  member_code VARCHAR(40) NOT NULL UNIQUE,
  title VARCHAR(24),
  first_name VARCHAR(80) NOT NULL,
  middle_name VARCHAR(80),
  last_name VARCHAR(80) NOT NULL,
  gender VARCHAR(12) CHECK (gender IN ('Male', 'Female', 'Other')),
  date_of_birth DATE,
  marital_status VARCHAR(20),
  occupation VARCHAR(120),
  phone VARCHAR(30) NOT NULL,
  email VARCHAR(180),
  address_line1 VARCHAR(180),
  city VARCHAR(80),
  state_region VARCHAR(80),
  emergency_contact_name VARCHAR(120),
  emergency_contact_phone VARCHAR(30),
  date_joined DATE,
  baptism_status VARCHAR(20) DEFAULT 'Pending' CHECK (baptism_status IN ('Pending', 'Completed', 'Not Applicable')),
  worker_status BOOLEAN NOT NULL DEFAULT FALSE,
  member_status VARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (member_status IN ('Active', 'Inactive', 'Transferred', 'Deceased')),
  photo_url TEXT,
  notes TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE departments
  ADD CONSTRAINT fk_departments_leader_member
  FOREIGN KEY (leader_member_id) REFERENCES members(member_id) ON DELETE SET NULL;

ALTER TABLE fellowships
  ADD CONSTRAINT fk_fellowships_leader_member
  FOREIGN KEY (leader_member_id) REFERENCES members(member_id) ON DELETE SET NULL;

CREATE TABLE IF NOT EXISTS member_status_history (
  member_status_history_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  member_id UUID NOT NULL REFERENCES members(member_id) ON DELETE CASCADE,
  old_status VARCHAR(20),
  new_status VARCHAR(20) NOT NULL,
  changed_by_user_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
  changed_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  reason TEXT
);

CREATE TABLE IF NOT EXISTS member_dependents (
  dependent_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  member_id UUID NOT NULL REFERENCES members(member_id) ON DELETE CASCADE,
  full_name VARCHAR(160) NOT NULL,
  relationship VARCHAR(40),
  date_of_birth DATE,
  phone VARCHAR(30),
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS visitors (
  visitor_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID NOT NULL REFERENCES branches(branch_id) ON DELETE RESTRICT,
  member_id_if_converted UUID REFERENCES members(member_id) ON DELETE SET NULL,
  visitor_code VARCHAR(40) NOT NULL UNIQUE,
  first_name VARCHAR(80) NOT NULL,
  last_name VARCHAR(80),
  phone VARCHAR(30),
  email VARCHAR(180),
  address_line1 VARCHAR(180),
  invited_by VARCHAR(160),
  first_attendance_date DATE NOT NULL,
  first_attendance_service VARCHAR(120),
  follow_up_status VARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (follow_up_status IN ('Pending', 'Contacted', 'Assimilated', 'Closed')),
  is_returning_visitor BOOLEAN NOT NULL DEFAULT FALSE,
  converted_to_member BOOLEAN NOT NULL DEFAULT FALSE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS new_converts (
  new_convert_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID NOT NULL REFERENCES branches(branch_id) ON DELETE RESTRICT,
  member_id_if_registered UUID REFERENCES members(member_id) ON DELETE SET NULL,
  full_name VARCHAR(160) NOT NULL,
  phone VARCHAR(30),
  email VARCHAR(180),
  decision_date DATE NOT NULL,
  decision_event VARCHAR(160),
  assigned_counselor VARCHAR(160),
  baptism_status VARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (baptism_status IN ('Pending', 'Completed')),
  discipleship_status VARCHAR(20) NOT NULL DEFAULT 'Not Started' CHECK (discipleship_status IN ('Not Started', 'In Progress', 'Completed')),
  assimilation_status VARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (assimilation_status IN ('Pending', 'In Progress', 'Completed')),
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS attendance_sessions (
  attendance_session_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID NOT NULL REFERENCES branches(branch_id) ON DELETE RESTRICT,
  session_title VARCHAR(160) NOT NULL,
  session_type VARCHAR(40) NOT NULL CHECK (
    session_type IN (
      'Sunday Service',
      'Midweek Service',
      'Workers Meeting',
      'Bible Study',
      'Fellowship Meeting',
      'Department Meeting',
      'Event Attendance',
      'Training Attendance'
    )
  ),
  session_date DATE NOT NULL,
  created_by_user_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS attendance_records (
  attendance_record_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  attendance_session_id UUID NOT NULL REFERENCES attendance_sessions(attendance_session_id) ON DELETE CASCADE,
  member_id UUID REFERENCES members(member_id) ON DELETE SET NULL,
  visitor_id UUID REFERENCES visitors(visitor_id) ON DELETE SET NULL,
  person_name VARCHAR(160) NOT NULL,
  person_type VARCHAR(20) NOT NULL CHECK (person_type IN ('Member', 'Visitor')),
  is_present BOOLEAN NOT NULL,
  recorded_by_user_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
  recorded_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT ck_attendance_member_or_visitor CHECK (
    (member_id IS NOT NULL AND visitor_id IS NULL AND person_type = 'Member')
    OR
    (visitor_id IS NOT NULL AND member_id IS NULL AND person_type = 'Visitor')
    OR
    (member_id IS NULL AND visitor_id IS NULL)
  )
);

CREATE TABLE IF NOT EXISTS pastoral_care_records (
  pastoral_care_record_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID NOT NULL REFERENCES branches(branch_id) ON DELETE RESTRICT,
  member_id UUID REFERENCES members(member_id) ON DELETE SET NULL,
  visitor_id UUID REFERENCES visitors(visitor_id) ON DELETE SET NULL,
  new_convert_id UUID REFERENCES new_converts(new_convert_id) ON DELETE SET NULL,
  category VARCHAR(30) NOT NULL CHECK (category IN ('Follow-Up', 'Prayer', 'Counseling', 'Home Visit', 'Welfare', 'Bereavement')),
  subject_name VARCHAR(160) NOT NULL,
  details TEXT,
  case_status VARCHAR(20) NOT NULL DEFAULT 'Open' CHECK (case_status IN ('Open', 'In Progress', 'Closed')),
  next_action_date DATE,
  confidentiality_level VARCHAR(16) NOT NULL DEFAULT 'Normal' CHECK (confidentiality_level IN ('Normal', 'Sensitive', 'Strict')),
  created_by_user_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
  assigned_to_user_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS follow_up_records (
  follow_up_record_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID NOT NULL REFERENCES branches(branch_id) ON DELETE RESTRICT,
  visitor_id UUID REFERENCES visitors(visitor_id) ON DELETE SET NULL,
  new_convert_id UUID REFERENCES new_converts(new_convert_id) ON DELETE SET NULL,
  member_id UUID REFERENCES members(member_id) ON DELETE SET NULL,
  action_type VARCHAR(30) NOT NULL CHECK (action_type IN ('Call', 'SMS', 'Visit', 'Counseling', 'Prayer')),
  outcome VARCHAR(30) CHECK (outcome IN ('Successful', 'No Response', 'Rescheduled', 'Escalated')),
  notes TEXT,
  action_date TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  next_action_date DATE,
  assigned_user_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
  status VARCHAR(20) NOT NULL DEFAULT 'Open' CHECK (status IN ('Open', 'In Progress', 'Closed')),
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS classes (
  class_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID NOT NULL REFERENCES branches(branch_id) ON DELETE RESTRICT,
  class_name VARCHAR(160) NOT NULL,
  class_type VARCHAR(40) NOT NULL CHECK (class_type IN ('Membership', 'Baptism', 'Discipleship', 'Workers Training')),
  facilitator_name VARCHAR(160),
  start_date DATE,
  end_date DATE,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(branch_id, class_name)
);

CREATE TABLE IF NOT EXISTS class_enrollments (
  class_enrollment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  class_id UUID NOT NULL REFERENCES classes(class_id) ON DELETE CASCADE,
  member_id UUID REFERENCES members(member_id) ON DELETE SET NULL,
  new_convert_id UUID REFERENCES new_converts(new_convert_id) ON DELETE SET NULL,
  participant_name VARCHAR(160) NOT NULL,
  enrollment_status VARCHAR(20) NOT NULL DEFAULT 'Enrolled' CHECK (enrollment_status IN ('Enrolled', 'Completed', 'Dropped')),
  completion_date DATE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS class_attendance (
  class_attendance_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  class_id UUID NOT NULL REFERENCES classes(class_id) ON DELETE CASCADE,
  class_enrollment_id UUID REFERENCES class_enrollments(class_enrollment_id) ON DELETE SET NULL,
  attendance_date DATE NOT NULL,
  is_present BOOLEAN NOT NULL,
  recorded_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS events (
  event_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID NOT NULL REFERENCES branches(branch_id) ON DELETE RESTRICT,
  event_name VARCHAR(180) NOT NULL,
  event_type VARCHAR(80),
  start_date DATE NOT NULL,
  end_date DATE,
  location VARCHAR(180),
  description TEXT,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS event_registrations (
  event_registration_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  event_id UUID NOT NULL REFERENCES events(event_id) ON DELETE CASCADE,
  member_id UUID REFERENCES members(member_id) ON DELETE SET NULL,
  visitor_id UUID REFERENCES visitors(visitor_id) ON DELETE SET NULL,
  registrant_name VARCHAR(160) NOT NULL,
  registrant_phone VARCHAR(30),
  registered_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  status VARCHAR(20) NOT NULL DEFAULT 'Registered' CHECK (status IN ('Registered', 'Cancelled', 'Attended'))
);

CREATE TABLE IF NOT EXISTS event_volunteers (
  event_volunteer_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  event_id UUID NOT NULL REFERENCES events(event_id) ON DELETE CASCADE,
  member_id UUID REFERENCES members(member_id) ON DELETE SET NULL,
  volunteer_name VARCHAR(160) NOT NULL,
  assigned_role VARCHAR(120),
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS contributions (
  contribution_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID NOT NULL REFERENCES branches(branch_id) ON DELETE RESTRICT,
  member_id UUID REFERENCES members(member_id) ON DELETE SET NULL,
  contribution_type VARCHAR(20) NOT NULL CHECK (contribution_type IN ('Tithe', 'Offering', 'Pledge', 'Special Donation')),
  amount NUMERIC(14,2) NOT NULL CHECK (amount >= 0),
  contribution_date DATE NOT NULL,
  payment_method VARCHAR(30),
  reference_number VARCHAR(120),
  notes TEXT,
  created_by_user_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS message_logs (
  message_log_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID REFERENCES branches(branch_id) ON DELETE SET NULL,
  channel VARCHAR(10) NOT NULL CHECK (channel IN ('SMS', 'Email')),
  subject VARCHAR(180),
  message_body TEXT NOT NULL,
  recipient_count INTEGER NOT NULL DEFAULT 0,
  status VARCHAR(20) NOT NULL DEFAULT 'Queued' CHECK (status IN ('Queued', 'Sent', 'Partial', 'Failed')),
  sent_by_user_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
  sent_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS notifications (
  notification_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID REFERENCES branches(branch_id) ON DELETE SET NULL,
  user_id UUID REFERENCES users(user_id) ON DELETE CASCADE,
  notification_type VARCHAR(40) NOT NULL CHECK (
    notification_type IN ('Birthday', 'Anniversary', 'Follow-Up', 'Absentee', 'Class Reminder', 'Event Reminder', 'System')
  ),
  title VARCHAR(180) NOT NULL,
  message TEXT NOT NULL,
  is_read BOOLEAN NOT NULL DEFAULT FALSE,
  due_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  read_at TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS audit_logs (
  audit_log_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  branch_id UUID REFERENCES branches(branch_id) ON DELETE SET NULL,
  user_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
  entity_name VARCHAR(80) NOT NULL,
  entity_id UUID,
  action_name VARCHAR(20) NOT NULL CHECK (action_name IN ('CREATE', 'UPDATE', 'DELETE', 'LOGIN', 'LOGOUT', 'EXPORT', 'VIEW')),
  action_summary TEXT,
  ip_address INET,
  user_agent TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_members_branch_status ON members(branch_id, member_status);
CREATE INDEX IF NOT EXISTS idx_members_name ON members(last_name, first_name);
CREATE INDEX IF NOT EXISTS idx_visitors_branch_followup ON visitors(branch_id, follow_up_status);
CREATE INDEX IF NOT EXISTS idx_new_converts_branch_decision_date ON new_converts(branch_id, decision_date);
CREATE INDEX IF NOT EXISTS idx_attendance_sessions_branch_date ON attendance_sessions(branch_id, session_date);
CREATE INDEX IF NOT EXISTS idx_attendance_records_session_present ON attendance_records(attendance_session_id, is_present);
CREATE INDEX IF NOT EXISTS idx_follow_up_records_branch_status ON follow_up_records(branch_id, status);
CREATE INDEX IF NOT EXISTS idx_pastoral_care_branch_status ON pastoral_care_records(branch_id, case_status);
CREATE INDEX IF NOT EXISTS idx_notifications_user_unread ON notifications(user_id, is_read);
CREATE INDEX IF NOT EXISTS idx_audit_logs_entity_date ON audit_logs(entity_name, created_at DESC);

COMMIT;
