using MemberCare.Api.Contracts;
using MemberCare.Api.Domain;
using MemberCare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemberCare.Api.Controllers;

[Authorize]
[ApiController]
[Route("v1/visitors")]
public sealed class VisitorsController : ControllerBase
{
    private readonly VisitorService _visitorService;

    public VisitorsController(VisitorService visitorService)
    {
        _visitorService = visitorService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<Visitor>>(StatusCodes.Status200OK)]
    public IActionResult List([FromQuery] Guid? branchId, [FromQuery] string? followUpStatus)
    {
        return Ok(new { items = _visitorService.List(branchId, followUpStatus) });
    }

    [HttpPost]
    [Authorize(Policy = "VisitorManagement")]
    [ProducesResponseType<Visitor>(StatusCodes.Status201Created)]
    public IActionResult Create([FromBody] VisitorCreateRequest request)
    {
        var visitor = _visitorService.Create(request);
        return Created($"v1/visitors/{visitor.VisitorId}", visitor);
    }

    [HttpPost("{visitorId:guid}/convert")]
    [Authorize(Policy = "VisitorManagement")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ConvertToMember([FromRoute] Guid visitorId)
    {
        var member = _visitorService.ConvertToMember(visitorId);
        if (member is null)
        {
            return NotFound();
        }

        return Ok(new { visitorId, member });
    }
}
