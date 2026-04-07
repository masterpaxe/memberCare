using MemberCare.Api.Contracts;
using MemberCare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemberCare.Api.Controllers;

[Authorize]
[ApiController]
[Route("v1/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    [ProducesResponseType<DashboardSummaryResponse>(StatusCodes.Status200OK)]
    public IActionResult GetSummary([FromQuery] Guid? branchId)
    {
        return Ok(_dashboardService.GetSummary(branchId));
    }
}
