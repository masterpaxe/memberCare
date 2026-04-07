namespace MemberCare.Api.Contracts;

public sealed record AuthLoginRequest(string UsernameOrEmail, string Password);
public sealed record AuthTokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);

public sealed record DashboardSummaryResponse(
    int TotalMembers,
    int ActiveMembers,
    int Visitors,
    int NewConverts,
    int AttendanceRecords,
    int PendingFollowUp,
    int Absentees
);

public sealed record MemberCreateRequest(Guid BranchId, string FirstName, string LastName, string Phone, string? Email);
public sealed record MemberUpdateRequest(string? Phone, string? Email, string? MemberStatus);

public sealed record VisitorCreateRequest(Guid BranchId, string FirstName, string? LastName, string? Phone, DateOnly FirstAttendanceDate);
public sealed record NewConvertCreateRequest(Guid BranchId, string FullName, DateOnly DecisionDate, string? AssignedCounselor);

public sealed record AttendanceSessionCreateRequest(Guid BranchId, string SessionTitle, string SessionType, DateOnly SessionDate);
public sealed record AttendanceRecordCreateRequest(Guid AttendanceSessionId, string PersonName, string PersonType, bool IsPresent);

public sealed record FollowUpCreateRequest(Guid BranchId, string ActionType, string Status);

public sealed record ReportSummaryResponse(DateTimeOffset GeneratedAt, Dictionary<string, object> Metrics);

public sealed record PagedResponse<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int Total);
