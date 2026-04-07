using MemberCare.Api.Domain;

namespace MemberCare.Api.Services;

public sealed class InMemoryStore
{
    public List<Member> Members { get; } = [];
    public List<Visitor> Visitors { get; } = [];
    public List<NewConvert> NewConverts { get; } = [];
    public List<AttendanceSession> AttendanceSessions { get; } = [];
    public List<AttendanceRecord> AttendanceRecords { get; } = [];
    public List<FollowUpRecord> FollowUps { get; } = [];

    public InMemoryStore()
    {
        var centralBranch = Guid.Parse("11111111-1111-1111-1111-111111111111");

        Members.Add(new Member
        {
            MemberId = Guid.NewGuid(),
            BranchId = centralBranch,
            MemberCode = "MEM-0001",
            FirstName = "Grace",
            LastName = "Johnson",
            Phone = "08030001000",
            Email = "grace.j@example.org",
            MemberStatus = "Active"
        });

        Visitors.Add(new Visitor
        {
            VisitorId = Guid.NewGuid(),
            BranchId = centralBranch,
            VisitorCode = "VIS-0001",
            FirstName = "Ruth",
            LastName = "Daniel",
            Phone = "08030003000",
            FirstAttendanceDate = DateOnly.FromDateTime(DateTime.UtcNow),
            FollowUpStatus = "Pending"
        });
    }
}
