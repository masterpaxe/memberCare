using MemberCare.Api.Contracts;
using MemberCare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemberCare.Api.Controllers;

[Authorize]
[ApiController]
[Route("v1/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("summary")]
    [Authorize(Policy = "Reports")]
    [ProducesResponseType<ReportSummaryResponse>(StatusCodes.Status200OK)]
    public IActionResult Summary([FromQuery] Guid? branchId, [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
    {
        _ = fromDate;
        _ = toDate;
        return Ok(_reportService.GetSummary(branchId));
    }
}
