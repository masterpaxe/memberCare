using MemberCare.Api.Contracts;
using Dapper;

namespace MemberCare.Api.Services;

public sealed class DashboardService
{
    private readonly SqlConnectionFactory _connectionFactory;

    public DashboardService(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public DashboardSummaryResponse GetSummary(Guid? branchId)
    {
        using var conn = _connectionFactory.CreateOpenConnection();
        var args = new DynamicParameters();
        args.Add("BranchId", branchId);

        const string memberFilter = "(@BranchId IS NULL OR branch_id = @BranchId)";
        const string attendanceFilter = "(@BranchId IS NULL OR s.branch_id = @BranchId)";

        var totalMembers = conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM members WHERE {memberFilter}", args);
        var activeMembers = conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM members WHERE {memberFilter} AND member_status = 'Active'", args);
        var visitors = conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM visitors WHERE {memberFilter}", args);
        var converts = conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM new_converts WHERE {memberFilter}", args);
        var pendingFollowup = conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM follow_up_records WHERE {memberFilter} AND status <> 'Closed'", args);
        var attendanceRecords = conn.ExecuteScalar<int>($@"
            SELECT COUNT(*)
            FROM attendance_records r
            JOIN attendance_sessions s ON s.attendance_session_id = r.attendance_session_id
            WHERE {attendanceFilter}", args);
        var absentees = conn.ExecuteScalar<int>($@"
            SELECT COUNT(*)
            FROM attendance_records r
            JOIN attendance_sessions s ON s.attendance_session_id = r.attendance_session_id
            WHERE {attendanceFilter} AND r.is_present = FALSE", args);

        return new DashboardSummaryResponse(totalMembers, activeMembers, visitors, converts, attendanceRecords, pendingFollowup, absentees);
    }
}
