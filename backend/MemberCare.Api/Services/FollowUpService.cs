using MemberCare.Api.Contracts;
using MemberCare.Api.Domain;
using Dapper;

namespace MemberCare.Api.Services;

public sealed class FollowUpService
{
    private readonly SqlConnectionFactory _connectionFactory;

    public FollowUpService(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IReadOnlyCollection<FollowUpRecord> List(Guid? branchId, string? status)
    {
        var where = new List<string>();
        var args = new DynamicParameters();
        if (branchId.HasValue)
        {
            where.Add("branch_id = @BranchId");
            args.Add("BranchId", branchId.Value);
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            where.Add("status = @Status");
            args.Add("Status", status);
        }

        var whereClause = where.Count > 0 ? $"WHERE {string.Join(" AND ", where)}" : string.Empty;

        using var conn = _connectionFactory.CreateOpenConnection();
        return conn.Query<FollowUpRecord>($@"
            SELECT
                follow_up_record_id AS FollowUpRecordId,
                branch_id AS BranchId,
                action_type AS ActionType,
                status AS Status,
                action_date AS ActionDate
            FROM follow_up_records
            {whereClause}
            ORDER BY action_date DESC", args).ToList();
    }

    public FollowUpRecord Create(FollowUpCreateRequest request)
    {
        using var conn = _connectionFactory.CreateOpenConnection();
        return conn.QuerySingle<FollowUpRecord>(@"
            INSERT INTO follow_up_records
                (follow_up_record_id, branch_id, action_type, status, action_date)
            VALUES
                (@FollowUpRecordId, @BranchId, @ActionType, @Status, NOW())
            RETURNING
                follow_up_record_id AS FollowUpRecordId,
                branch_id AS BranchId,
                action_type AS ActionType,
                status AS Status,
                action_date AS ActionDate",
            new
            {
                FollowUpRecordId = Guid.NewGuid(),
                request.BranchId,
                request.ActionType,
                request.Status
            });
    }
}
