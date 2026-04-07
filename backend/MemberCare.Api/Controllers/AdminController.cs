using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemberCare.Api.Controllers;

[Authorize]
[ApiController]
[Route("v1/admin")]
public sealed class AdminController : ControllerBase
{
    [HttpGet("users")]
    [Authorize(Policy = "ChurchAdmin")]
    public IActionResult ListUsers()
    {
        return Ok(new
        {
            items = new[]
            {
                new
                {
                    userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    username = "admin",
                    email = "admin@membercare.local",
                    firstName = "System",
                    lastName = "Administrator",
                    status = "Active"
                }
            }
        });
    }
}
