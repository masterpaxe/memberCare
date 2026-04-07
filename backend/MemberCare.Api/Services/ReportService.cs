using MemberCare.Api.Contracts;

namespace MemberCare.Api.Services;

public sealed class ReportService
{
    private readonly DashboardService _dashboardService;

    public ReportService(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public ReportSummaryResponse GetSummary(Guid? branchId)
    {
        var summary = _dashboardService.GetSummary(branchId);

        var metrics = new Dictionary<string, object>
        {
            ["members.total"] = summary.TotalMembers,
            ["members.active"] = summary.ActiveMembers,
            ["visitors.total"] = summary.Visitors,
            ["converts.total"] = summary.NewConverts,
            ["followup.open"] = summary.PendingFollowUp,
            ["attendance.records"] = summary.AttendanceRecords,
            ["attendance.absentees"] = summary.Absentees
        };

        return new ReportSummaryResponse(DateTimeOffset.UtcNow, metrics);
    }
}
