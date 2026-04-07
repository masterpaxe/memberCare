using MemberCare.Api.Contracts;
using MemberCare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemberCare.Api.Controllers;

[ApiController]
[Route("v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AuthTokenResponse>(StatusCodes.Status200OK)]
    public IActionResult Login([FromBody] AuthLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { code = "validation_error", message = "Username and password are required." });
        }

        var token = _authService.Login(request);
        return Ok(token);
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] Dictionary<string, string> request)
    {
        _ = request;
        return Ok(new AuthTokenResponse("dev-access-token", "dev-refresh-token", 3600));
    }
}
