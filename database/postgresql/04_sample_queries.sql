-- Sample operational and leadership queries

-- 1) Members by branch and status
SELECT b.name AS branch_name, m.member_status, COUNT(*) AS total
FROM members m
JOIN branches b ON b.branch_id = m.branch_id
GROUP BY b.name, m.member_status
ORDER BY b.name, m.member_status;

-- 2) Visitors pending follow-up in a branch
SELECT v.visitor_code, v.first_name, v.last_name, v.first_attendance_date, v.follow_up_status
FROM visitors v
JOIN branches b ON b.branch_id = v.branch_id
WHERE b.code = 'CENTRAL' AND v.follow_up_status = 'Pending'
ORDER BY v.first_attendance_date DESC;

-- 3) New converts with pending baptism
SELECT nc.full_name, nc.decision_date, nc.assigned_counselor, nc.baptism_status
FROM new_converts nc
WHERE nc.baptism_status = 'Pending'
ORDER BY nc.decision_date DESC;

-- 4) Absentees by attendance session
SELECT s.session_title, s.session_date, r.person_name, r.person_type
FROM attendance_records r
JOIN attendance_sessions s ON s.attendance_session_id = r.attendance_session_id
WHERE r.is_present = FALSE
ORDER BY s.session_date DESC, r.person_name;

-- 5) Open pastoral care and follow-up workload by branch
SELECT b.name AS branch_name,
       COALESCE(p.open_care_cases, 0) AS open_care_cases,
       COALESCE(f.open_followup_cases, 0) AS open_followup_cases
FROM branches b
LEFT JOIN (
  SELECT branch_id, COUNT(*) AS open_care_cases
  FROM pastoral_care_records
  WHERE case_status <> 'Closed'
  GROUP BY branch_id
) p ON p.branch_id = b.branch_id
LEFT JOIN (
  SELECT branch_id, COUNT(*) AS open_followup_cases
  FROM follow_up_records
  WHERE status <> 'Closed'
  GROUP BY branch_id
) f ON f.branch_id = b.branch_id
ORDER BY b.name;
