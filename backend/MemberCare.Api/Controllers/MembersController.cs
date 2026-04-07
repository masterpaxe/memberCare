using MemberCare.Api.Contracts;
using MemberCare.Api.Domain;
using MemberCare.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemberCare.Api.Controllers;

[ApiController]
[Route("v1/members")]
public sealed class MembersController : ControllerBase
{
    private readonly MemberService _memberService;

    public MembersController(MemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    [ProducesResponseType<PagedResponse<Member>>(StatusCodes.Status200OK)]
    public IActionResult List(
        [FromQuery] Guid? branchId,
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        return Ok(_memberService.List(branchId, status, search, page, pageSize));
    }

    [HttpGet("{memberId:guid}")]
    [ProducesResponseType<Member>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get([FromRoute] Guid memberId)
    {
        var member = _memberService.Get(memberId);
        return member is null ? NotFound() : Ok(member);
    }

    [HttpPost]
    [ProducesResponseType<Member>(StatusCodes.Status201Created)]
    public IActionResult Create([FromBody] MemberCreateRequest request)
    {
        var member = _memberService.Create(request);
        return CreatedAtAction(nameof(Get), new { memberId = member.MemberId }, member);
    }

    [HttpPatch("{memberId:guid}")]
    [ProducesResponseType<Member>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update([FromRoute] Guid memberId, [FromBody] MemberUpdateRequest request)
    {
        var member = _memberService.Update(memberId, request);
        return member is null ? NotFound() : Ok(member);
    }

    [HttpDelete("{memberId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete([FromRoute] Guid memberId)
    {
        var removed = _memberService.Delete(memberId);
        return removed ? NoContent() : NotFound();
    }
}
