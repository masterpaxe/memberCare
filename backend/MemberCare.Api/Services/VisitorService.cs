using MemberCare.Api.Contracts;
using MemberCare.Api.Domain;
using Dapper;

namespace MemberCare.Api.Services;

public sealed class VisitorService
{
    private readonly SqlConnectionFactory _connectionFactory;
    private readonly BranchContext _branchContext;

    public VisitorService(SqlConnectionFactory connectionFactory, BranchContext branchContext)
    {
        _connectionFactory = connectionFactory;
        _branchContext = branchContext;
    }

    public IReadOnlyCollection<Visitor> List(Guid? branchId, string? followUpStatus)
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

        if (!string.IsNullOrWhiteSpace(followUpStatus))
        {
            where.Add("follow_up_status = @FollowUpStatus");
            args.Add("FollowUpStatus", followUpStatus);
        }

        var whereClause = where.Count > 0 ? $"WHERE {string.Join(" AND ", where)}" : string.Empty;

        using var conn = _connectionFactory.CreateOpenConnection();
        return conn.Query<Visitor>($@"
            SELECT
                visitor_id AS VisitorId,
                branch_id AS BranchId,
                visitor_code AS VisitorCode,
                first_name AS FirstName,
                last_name AS LastName,
                phone AS Phone,
                first_attendance_date AS FirstAttendanceDate,
                follow_up_status AS FollowUpStatus
            FROM visitors
            {whereClause}
            ORDER BY created_at DESC", args).ToList();
    }

    public Visitor Create(VisitorCreateRequest request)
    {
        var userBranchId = _branchContext.GetUserBranchId();

        // Enforce branch scoping: non-super-admin can only create in their assigned branch
        if (userBranchId.HasValue && request.BranchId != userBranchId.Value)
        {
            throw new UnauthorizedAccessException("Cannot create visitors outside your assigned branch.");
        }

        using var conn = _connectionFactory.CreateOpenConnection();
        return conn.QuerySingle<Visitor>(@"
            INSERT INTO visitors
                (visitor_id, branch_id, visitor_code, first_name, last_name, phone, first_attendance_date, follow_up_status)
            VALUES
                (@VisitorId, @BranchId, @VisitorCode, @FirstName, @LastName, @Phone, @FirstAttendanceDate, 'Pending')
            RETURNING
                visitor_id AS VisitorId,
                branch_id AS BranchId,
                visitor_code AS VisitorCode,
                first_name AS FirstName,
                last_name AS LastName,
                phone AS Phone,
                first_attendance_date AS FirstAttendanceDate,
                follow_up_status AS FollowUpStatus",
            new
            {
                VisitorId = Guid.NewGuid(),
                request.BranchId,
                VisitorCode = $"VIS-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                request.FirstName,
                request.LastName,
                request.Phone,
                request.FirstAttendanceDate
            });
    }

    public Member? ConvertToMember(Guid visitorId)
    {
        var userBranchId = _branchContext.GetUserBranchId();

        using var conn = _connectionFactory.CreateOpenConnection();

        // If not super_admin, verify visitor belongs to user's branch before allowing conversion
        if (userBranchId.HasValue)
        {
            var visitorBranch = conn.ExecuteScalar<Guid?>("SELECT branch_id FROM visitors WHERE visitor_id = @VisitorId", new { VisitorId = visitorId });
            if (visitorBranch != userBranchId.Value)
            {
                return null; // Forbidden: visitor not in user's branch
            }
        }

        using var tx = conn.BeginTransaction();

        var source = conn.QuerySingleOrDefault<(Guid BranchId, string FirstName, string? LastName, string? Phone)>(@"
            UPDATE visitors
            SET
                follow_up_status = 'Assimilated',
                converted_to_member = TRUE,
                updated_at = NOW()
            WHERE visitor_id = @VisitorId
            RETURNING branch_id AS BranchId, first_name AS FirstName, last_name AS LastName, phone AS Phone",
            new { VisitorId = visitorId }, tx);

        if (source.BranchId == Guid.Empty)
        {
            tx.Rollback();
            return null;
        }

        var member = conn.QuerySingle<Member>(@"
            INSERT INTO members
                (member_id, branch_id, member_code, first_name, last_name, phone, member_status)
            VALUES
                (@MemberId, @BranchId, @MemberCode, @FirstName, @LastName, @Phone, 'Active')
            RETURNING
                member_id AS MemberId,
                branch_id AS BranchId,
                member_code AS MemberCode,
                first_name AS FirstName,
                last_name AS LastName,
                phone AS Phone,
                email AS Email,
                member_status AS MemberStatus",
            new
            {
                MemberId = Guid.NewGuid(),
                source.BranchId,
                MemberCode = $"MEM-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                FirstName = source.FirstName,
                LastName = string.IsNullOrWhiteSpace(source.LastName) ? "Visitor" : source.LastName,
                Phone = source.Phone ?? string.Empty
            }, tx);

        tx.Commit();
        return member;
    }
}
