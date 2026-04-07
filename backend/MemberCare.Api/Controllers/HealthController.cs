using Dapper;
using MemberCare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemberCare.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("v1/health")]
public sealed class HealthController : ControllerBase
{
    private readonly SqlConnectionFactory _connectionFactory;

    public HealthController(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Get()
    {
        try
        {
            using var conn = _connectionFactory.CreateOpenConnection();
            var dbOk = conn.ExecuteScalar<int>("SELECT 1") == 1;

            if (!dbOk)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = "unhealthy",
                    database = "unreachable",
                    timestampUtc = DateTimeOffset.UtcNow
                });
            }

            return Ok(new
            {
                status = "healthy",
                database = "ok",
                timestampUtc = DateTimeOffset.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "unhealthy",
                database = "unreachable",
                error = ex.Message,
                timestampUtc = DateTimeOffset.UtcNow
            });
        }
    }
}
