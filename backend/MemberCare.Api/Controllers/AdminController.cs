using Microsoft.AspNetCore.Mvc;

namespace MemberCare.Api.Controllers;

[ApiController]
[Route("v1/admin")]
public sealed class AdminController : ControllerBase
{
    [HttpGet("users")]
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
