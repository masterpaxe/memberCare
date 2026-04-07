using MemberCare.Api.Contracts;
using MemberCare.Api.Domain;
using MemberCare.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemberCare.Api.Controllers;

[ApiController]
[Route("v1/attendance")]
public sealed class AttendanceController : ControllerBase
{
    private readonly AttendanceService _attendanceService;

    public AttendanceController(AttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet("sessions")]
    [ProducesResponseType<IReadOnlyCollection<AttendanceSession>>(StatusCodes.Status200OK)]
    public IActionResult ListSessions([FromQuery] Guid? branchId, [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
    {
        return Ok(new { items = _attendanceService.ListSessions(branchId, fromDate, toDate) });
    }

    [HttpPost("sessions")]
    [ProducesResponseType<AttendanceSession>(StatusCodes.Status201Created)]
    public IActionResult CreateSession([FromBody] AttendanceSessionCreateRequest request)
    {
        var session = _attendanceService.CreateSession(request);
        return Created($"v1/attendance/sessions/{session.AttendanceSessionId}", session);
    }

    [HttpPost("records")]
    [ProducesResponseType<AttendanceRecord>(StatusCodes.Status201Created)]
    public IActionResult CreateRecord([FromBody] AttendanceRecordCreateRequest request)
    {
        var record = _attendanceService.CreateRecord(request);
        return Created($"v1/attendance/records/{record.AttendanceRecordId}", record);
    }
}
