using MemberCare.Api.Contracts;
using MemberCare.Api.Domain;
using Dapper;

namespace MemberCare.Api.Services;

public sealed class NewConvertService
{
    private readonly SqlConnectionFactory _connectionFactory;

    public NewConvertService(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IReadOnlyCollection<NewConvert> List(Guid? branchId, string? baptismStatus)
    {
        var where = new List<string>();
        var args = new DynamicParameters();
        if (branchId.HasValue)
        {
            where.Add("branch_id = @BranchId");
            args.Add("BranchId", branchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(baptismStatus))
        {
            where.Add("baptism_status = @BaptismStatus");
            args.Add("BaptismStatus", baptismStatus);
        }

        var whereClause = where.Count > 0 ? $"WHERE {string.Join(" AND ", where)}" : string.Empty;

        using var conn = _connectionFactory.CreateOpenConnection();
        return conn.Query<NewConvert>($@"
            SELECT
                new_convert_id AS NewConvertId,
                branch_id AS BranchId,
                full_name AS FullName,
                decision_date AS DecisionDate,
                baptism_status AS BaptismStatus
            FROM new_converts
            {whereClause}
            ORDER BY decision_date DESC", args).ToList();
    }

    public NewConvert Create(NewConvertCreateRequest request)
    {
        using var conn = _connectionFactory.CreateOpenConnection();
        return conn.QuerySingle<NewConvert>(@"
            INSERT INTO new_converts
                (new_convert_id, branch_id, full_name, decision_date, assigned_counselor, baptism_status)
            VALUES
                (@NewConvertId, @BranchId, @FullName, @DecisionDate, @AssignedCounselor, 'Pending')
            RETURNING
                new_convert_id AS NewConvertId,
                branch_id AS BranchId,
                full_name AS FullName,
                decision_date AS DecisionDate,
                baptism_status AS BaptismStatus",
            new
            {
                NewConvertId = Guid.NewGuid(),
                request.BranchId,
                request.FullName,
                request.DecisionDate,
                request.AssignedCounselor
            });
    }
}
