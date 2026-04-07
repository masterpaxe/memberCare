namespace MemberCare.Api.Domain;

public sealed class Member
{
    public Guid MemberId { get; init; }
    public Guid BranchId { get; init; }
    public string MemberCode { get; init; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string MemberStatus { get; set; } = "Active";
}

public sealed class Visitor
{
    public Guid VisitorId { get; init; }
    public Guid BranchId { get; init; }
    public string VisitorCode { get; init; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public DateOnly FirstAttendanceDate { get; set; }
    public string FollowUpStatus { get; set; } = "Pending";
}

public sealed class NewConvert
{
    public Guid NewConvertId { get; init; }
    public Guid BranchId { get; init; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly DecisionDate { get; set; }
    public string BaptismStatus { get; set; } = "Pending";
}

public sealed class AttendanceSession
{
    public Guid AttendanceSessionId { get; init; }
    public Guid BranchId { get; init; }
    public string SessionTitle { get; set; } = string.Empty;
    public string SessionType { get; set; } = "Sunday Service";
    public DateOnly SessionDate { get; set; }
}

public sealed class AttendanceRecord
{
    public Guid AttendanceRecordId { get; init; }
    public Guid AttendanceSessionId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string PersonType { get; set; } = "Member";
    public bool IsPresent { get; set; }
}

public sealed class FollowUpRecord
{
    public Guid FollowUpRecordId { get; init; }
    public Guid BranchId { get; init; }
    public string ActionType { get; set; } = "Call";
    public string Status { get; set; } = "Open";
    public DateTimeOffset ActionDate { get; set; }
}
