-- MemberCare PostgreSQL reporting views
-- Run after schema and seed scripts.

BEGIN;

CREATE OR REPLACE VIEW v_member_summary_by_branch AS
SELECT
  b.branch_id,
  b.name AS branch_name,
  COUNT(m.member_id) AS total_members,
  COUNT(*) FILTER (WHERE m.member_status = 'Active') AS active_members,
  COUNT(*) FILTER (WHERE m.member_status = 'Inactive') AS inactive_members
FROM branches b
LEFT JOIN members m ON m.branch_id = b.branch_id
GROUP BY b.branch_id, b.name;

CREATE OR REPLACE VIEW v_visitor_followup_summary AS
SELECT
  b.branch_id,
  b.name AS branch_name,
  COUNT(v.visitor_id) AS total_visitors,
  COUNT(*) FILTER (WHERE v.follow_up_status = 'Pending') AS pending_followup,
  COUNT(*) FILTER (WHERE v.follow_up_status = 'Assimilated') AS assimilated
FROM branches b
LEFT JOIN visitors v ON v.branch_id = b.branch_id
GROUP BY b.branch_id, b.name;

CREATE OR REPLACE VIEW v_attendance_session_stats AS
SELECT
  s.attendance_session_id,
  s.branch_id,
  s.session_title,
  s.session_type,
  s.session_date,
  COUNT(r.attendance_record_id) AS total_records,
  COUNT(*) FILTER (WHERE r.is_present = TRUE) AS present_count,
  COUNT(*) FILTER (WHERE r.is_present = FALSE) AS absent_count
FROM attendance_sessions s
LEFT JOIN attendance_records r ON r.attendance_session_id = s.attendance_session_id
GROUP BY s.attendance_session_id, s.branch_id, s.session_title, s.session_type, s.session_date;

CREATE OR REPLACE VIEW v_followup_open_cases AS
SELECT
  branch_id,
  COUNT(*) AS open_cases
FROM follow_up_records
WHERE status <> 'Closed'
GROUP BY branch_id;

COMMIT;
