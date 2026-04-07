using MemberCare.Api.Contracts;
using MemberCare.Api.Domain;
using MemberCare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemberCare.Api.Controllers;

[Authorize]
[ApiController]
[Route("v1/follow-up/records")]
public sealed class FollowUpController : ControllerBase
{
    private readonly FollowUpService _followUpService;

    public FollowUpController(FollowUpService followUpService)
    {
        _followUpService = followUpService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<FollowUpRecord>>(StatusCodes.Status200OK)]
    public IActionResult List([FromQuery] Guid? branchId, [FromQuery] string? status)
    {
        return Ok(new { items = _followUpService.List(branchId, status) });
    }

    [HttpPost]
    [Authorize(Policy = "FollowUpManagement")]
    [ProducesResponseType<FollowUpRecord>(StatusCodes.Status201Created)]
    public IActionResult Create([FromBody] FollowUpCreateRequest request)
    {
        var record = _followUpService.Create(request);
        return Created($"v1/follow-up/records/{record.FollowUpRecordId}", record);
    }
}
