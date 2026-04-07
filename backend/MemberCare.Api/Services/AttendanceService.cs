using MemberCare.Api.Contracts;
using MemberCare.Api.Domain;
using Dapper;

namespace MemberCare.Api.Services;

public sealed class AttendanceService
{
    private readonly SqlConnectionFactory _connectionFactory;
    private readonly BranchContext _branchContext;

    public AttendanceService(SqlConnectionFactory connectionFactory, BranchContext branchContext)
    {
        _connectionFactory = connectionFactory;
        _branchContext = branchContext;
    }

    public IReadOnlyCollection<AttendanceSession> ListSessions(Guid? branchId, DateOnly? fromDate, DateOnly? toDate)
    {
        var where = new List<string>();
        var args = new DynamicParameters();

        // Enforce branch scoping
        var userBranchId = _branchContext.GetUserBranchId();
        if (userBranchId.HasValue)
        {
            where.Add("branch_id = @UserBranchId");
            args.Add("UserBranchId", userBranchId.Value);
        }
        else if (branchId.HasValue)
        {
            where.Add("branch_id = @BranchId");
            args.Add("BranchId", branchId.Value);
        }

        if (fromDate.HasValue)
        {
            where.Add("session_date >= @FromDate");
            args.Add("FromDate", fromDate.Value);
        }

        if (toDate.HasValue)
        {
            where.Add("session_date <= @ToDate");
            args.Add("ToDate", toDate.Value);
        }

        var whereClause = where.Count > 0 ? $"WHERE {string.Join(" AND ", where)}" : string.Empty;

        using var conn = _connectionFactory.CreateOpenConnection();
        return conn.Query<AttendanceSession>($@"
            SELECT
                attendance_session_id AS AttendanceSessionId,
                branch_id AS BranchId,
                session_title AS SessionTitle,
                session_type AS SessionType,
                session_date AS SessionDate
            FROM attendance_sessions
            {whereClause}
            ORDER BY session_date DESC", args).ToList();
    }

    public AttendanceSession CreateSession(AttendanceSessionCreateRequest request)
    {
        var userBranchId = _branchContext.GetUserBranchId();

        // Enforce branch scoping: non-super-admin can only create in their assigned branch
        if (userBranchId.HasValue && request.BranchId != userBranchId.Value)
        {
            throw new UnauthorizedAccessException("Cannot create attendance sessions outside your assigned branch.");
        }

        using var conn = _connectionFactory.CreateOpenConnection();
        return conn.QuerySingle<AttendanceSession>(@"
            INSERT INTO attendance_sessions
                (attendance_session_id, branch_id, session_title, session_type, session_date)
            VALUES
                (@AttendanceSessionId, @BranchId, @SessionTitle, @SessionType, @SessionDate)
            RETURNING
                attendance_session_id AS AttendanceSessionId,
                branch_id AS BranchId,
                session_title AS SessionTitle,
                session_type AS SessionType,
                session_date AS SessionDate",
            new
            {
                AttendanceSessionId = Guid.NewGuid(),
                request.BranchId,
                request.SessionTitle,
                request.SessionType,
                request.SessionDate
            });
    }

    public AttendanceRecord CreateRecord(AttendanceRecordCreateRequest request)
    {
        var userBranchId = _branchContext.GetUserBranchId();

        using var conn = _connectionFactory.CreateOpenConnection();

        // If not super_admin, verify attendance session belongs to user's branch
        if (userBranchId.HasValue)
        {
            var sessionBranch = conn.ExecuteScalar<Guid?>(
                "SELECT branch_id FROM attendance_sessions WHERE attendance_session_id = @SessionId",
                new { SessionId = request.AttendanceSessionId });
            if (sessionBranch != userBranchId.Value)
            {
                throw new UnauthorizedAccessException("Cannot record attendance for sessions outside your assigned branch.");
            }
        }

        return conn.QuerySingle<AttendanceRecord>(@"
            INSERT INTO attendance_records
                (attendance_record_id, attendance_session_id, person_name, person_type, is_present)
            VALUES
                (@AttendanceRecordId, @AttendanceSessionId, @PersonName, @PersonType, @IsPresent)
            RETURNING
                attendance_record_id AS AttendanceRecordId,
                attendance_session_id AS AttendanceSessionId,
                person_name AS PersonName,
                person_type AS PersonType,
                is_present AS IsPresent",
            new
            {
                AttendanceRecordId = Guid.NewGuid(),
                request.AttendanceSessionId,
                request.PersonName,
                request.PersonType,
                request.IsPresent
            });
    }
}
