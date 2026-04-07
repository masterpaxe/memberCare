using MemberCare.Api.Contracts;
using MemberCare.Api.Domain;
using Dapper;

namespace MemberCare.Api.Services;

public sealed class MemberService
{
    private readonly SqlConnectionFactory _connectionFactory;
    private readonly BranchContext _branchContext;

    public MemberService(SqlConnectionFactory connectionFactory, BranchContext branchContext)
    {
        _connectionFactory = connectionFactory;
        _branchContext = branchContext;
    }

    public PagedResponse<Member> List(Guid? branchId, string? status, string? search, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var whereParts = new List<string>();
        var args = new DynamicParameters();

        // Enforce branch scoping: user can only see their assigned branch unless super_admin
        var userBranchId = _branchContext.GetUserBranchId();
        if (userBranchId.HasValue)
        {
            // Non-super-admin: filter to their branch only (ignore branchId parameter)
            whereParts.Add("branch_id = @UserBranchId");
            args.Add("UserBranchId", userBranchId.Value);
        }
        else if (branchId.HasValue)
        {
            // Super-admin: allow filtering by specified branch
            whereParts.Add("branch_id = @BranchId");
            args.Add("BranchId", branchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            whereParts.Add("member_status = @Status");
            args.Add("Status", status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            whereParts.Add("(member_code ILIKE @Search OR first_name ILIKE @Search OR last_name ILIKE @Search OR phone ILIKE @Search)");
            args.Add("Search", $"%{search}%");
        }

        var whereClause = whereParts.Count > 0 ? $"WHERE {string.Join(" AND ", whereParts)}" : string.Empty;
        args.Add("Offset", (page - 1) * pageSize);
        args.Add("PageSize", pageSize);

        const string projection = @"
            member_id AS MemberId,
            branch_id AS BranchId,
            member_code AS MemberCode,
            first_name AS FirstName,
            last_name AS LastName,
            phone AS Phone,
            email AS Email,
            member_status AS MemberStatus";

        using var conn = _connectionFactory.CreateOpenConnection();
        var total = conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM members {whereClause}", args);
        var items = conn.Query<Member>($@"
            SELECT {projection}
            FROM members
            {whereClause}
            ORDER BY created_at DESC
            OFFSET @Offset LIMIT @PageSize", args).ToList();

        return new PagedResponse<Member>(items, page, pageSize, total);
    }

    public Member? Get(Guid memberId)
    {
        var userBranchId = _branchContext.GetUserBranchId();
        using var conn = _connectionFactory.CreateOpenConnection();
        var member = conn.QuerySingleOrDefault<Member>(@"
            SELECT
                member_id AS MemberId,
                branch_id AS BranchId,
                member_code AS MemberCode,
                first_name AS FirstName,
                last_name AS LastName,
                phone AS Phone,
                email AS Email,
                member_status AS MemberStatus
            FROM members
            WHERE member_id = @MemberId", new { MemberId = memberId });

        // Enforce branch scoping: deny access if user is not super_admin and member is not in their branch
        if (member is not null && userBranchId.HasValue && member.BranchId != userBranchId.Value)
        {
            return null; // Forbidden: member not in user's branch
        }

        return member;
    }

    public Member Create(MemberCreateRequest request)
    {
        var userBranchId = _branchContext.GetUserBranchId();

        // Enforce branch scoping: non-super-admin can only create in their assigned branch
        if (userBranchId.HasValue && request.BranchId != userBranchId.Value)
        {
            throw new UnauthorizedAccessException("Cannot create members outside your assigned branch.");
        }

        using var conn = _connectionFactory.CreateOpenConnection();
        return conn.QuerySingle<Member>(@"
            INSERT INTO members
                (member_id, branch_id, member_code, first_name, last_name, phone, email, member_status)
            VALUES
                (@MemberId, @BranchId, @MemberCode, @FirstName, @LastName, @Phone, @Email, 'Active')
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
                request.BranchId,
                MemberCode = $"MEM-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                request.FirstName,
                request.LastName,
                request.Phone,
                request.Email
            });
    }

    public Member? Update(Guid memberId, MemberUpdateRequest request)
    {
        var userBranchId = _branchContext.GetUserBranchId();

        using var conn = _connectionFactory.CreateOpenConnection();
        
        // If not super_admin, verify member belongs to user's branch before allowing update
        if (userBranchId.HasValue)
        {
            var memberBranch = conn.ExecuteScalar<Guid?>("SELECT branch_id FROM members WHERE member_id = @MemberId", new { MemberId = memberId });
            if (memberBranch != userBranchId.Value)
            {
                return null; // Forbidden: member not in user's branch
            }
        }

        return conn.QuerySingleOrDefault<Member>(@"
            UPDATE members
            SET
                phone = COALESCE(@Phone, phone),
                email = COALESCE(@Email, email),
                member_status = COALESCE(@MemberStatus, member_status),
                updated_at = NOW()
            WHERE member_id = @MemberId
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
                MemberId = memberId,
                request.Phone,
                request.Email,
                request.MemberStatus
            });
    }

    public bool Delete(Guid memberId)
    {
        var userBranchId = _branchContext.GetUserBranchId();

        using var conn = _connectionFactory.CreateOpenConnection();

        // If not super_admin, verify member belongs to user's branch before allowing delete
        if (userBranchId.HasValue)
        {
            var memberBranch = conn.ExecuteScalar<Guid?>("SELECT branch_id FROM members WHERE member_id = @MemberId", new { MemberId = memberId });
            if (memberBranch != userBranchId.Value)
            {
                return false; // Forbidden: member not in user's branch
            }
        }

        var affected = conn.Execute("DELETE FROM members WHERE member_id = @MemberId", new { MemberId = memberId });
        return affected > 0;
    }
}
