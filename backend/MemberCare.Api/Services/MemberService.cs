using MemberCare.Api.Contracts;
using MemberCare.Api.Domain;
using Dapper;

namespace MemberCare.Api.Services;

public sealed class MemberService
{
    private readonly SqlConnectionFactory _connectionFactory;

    public MemberService(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public PagedResponse<Member> List(Guid? branchId, string? status, string? search, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var whereParts = new List<string>();
        var args = new DynamicParameters();

        if (branchId.HasValue)
        {
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
        using var conn = _connectionFactory.CreateOpenConnection();
        return conn.QuerySingleOrDefault<Member>(@"
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
    }

    public Member Create(MemberCreateRequest request)
    {
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
        using var conn = _connectionFactory.CreateOpenConnection();
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
        using var conn = _connectionFactory.CreateOpenConnection();
        var affected = conn.Execute("DELETE FROM members WHERE member_id = @MemberId", new { MemberId = memberId });
        return affected > 0;
    }
}
