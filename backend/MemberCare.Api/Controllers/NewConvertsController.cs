using MemberCare.Api.Contracts;
using MemberCare.Api.Domain;
using MemberCare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemberCare.Api.Controllers;

[Authorize]
[ApiController]
[Route("v1/new-converts")]
public sealed class NewConvertsController : ControllerBase
{
    private readonly NewConvertService _newConvertService;

    public NewConvertsController(NewConvertService newConvertService)
    {
        _newConvertService = newConvertService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<NewConvert>>(StatusCodes.Status200OK)]
    public IActionResult List([FromQuery] Guid? branchId, [FromQuery] string? baptismStatus)
    {
        return Ok(new { items = _newConvertService.List(branchId, baptismStatus) });
    }

    [HttpPost]
    [Authorize(Policy = "VisitorManagement")]
    [ProducesResponseType<NewConvert>(StatusCodes.Status201Created)]
    public IActionResult Create([FromBody] NewConvertCreateRequest request)
    {
        var convert = _newConvertService.Create(request);
        return Created($"v1/new-converts/{convert.NewConvertId}", convert);
    }
}
