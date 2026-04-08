namespace MemberCare.Web.Models;

public record AuthTokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);

public record DashboardSummaryDto(
    int TotalMembers,
    int ActiveMembers,
    int Visitors,
    int NewConverts,
    int AttendanceRecords,
    int PendingFollowUp,
    int Absentees);

public record ItemsResponse<T>(List<T> Items);

public record MemberDto(
    Guid MemberId,
    string FirstName,
    string LastName,
    string Phone,
    string? Email,
    string MemberStatus,
    Guid BranchId);

public record VisitorDto(
    Guid VisitorId,
    string FirstName,
    string? LastName,
    string? Phone,
    string FirstAttendanceDate,
    string FollowUpStatus,
    Guid BranchId);

public record NewConvertDto(
    Guid NewConvertId,
    string FullName,
    string DecisionDate,
    string? AssignedCounselor,
    string BaptismStatus,
    Guid BranchId);

public record AttendanceSessionDto(
    Guid AttendanceSessionId,
    string SessionTitle,
    string SessionType,
    string SessionDate,
    Guid BranchId);

public record AttendanceRecordDto(
    Guid AttendanceRecordId,
    Guid AttendanceSessionId,
    string PersonName,
    string PersonType,
    bool IsPresent);

public record FollowUpDto(
    Guid FollowUpRecordId,
    string ActionType,
    string Status,
    string ActionDate,
    Guid BranchId);
