using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _service;

    public AppointmentsController(IAppointmentService service) => _service = service;

    /// <summary>List appointments with filters and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AppointmentResponseDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? status,
        [FromQuery] int? vetId,
        [FromQuery] int? petId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(from, to, status, vetId, petId, new PaginationParams { Page = page, PageSize = pageSize }));

    /// <summary>Get appointment details including pet, vet, and medical record</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AppointmentDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var appt = await _service.GetByIdAsync(id);
        return appt is null ? NotFound() : Ok(appt);
    }

    /// <summary>Schedule a new appointment with conflict detection</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AppointmentResponseDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
    {
        var (result, error) = await _service.CreateAsync(dto);
        if (error is not null)
        {
            var statusCode = error.Contains("conflict", StringComparison.OrdinalIgnoreCase) ? 409 : 400;
            return StatusCode(statusCode, new ProblemDetails { Title = "Appointment creation failed", Detail = error, Status = statusCode });
        }
        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    /// <summary>Update appointment details (re-checks conflicts)</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AppointmentResponseDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] AppointmentUpdateDto dto)
    {
        var (result, error) = await _service.UpdateAsync(id, dto);
        if (error == "Appointment not found.") return NotFound();
        if (error is not null)
        {
            var statusCode = error.Contains("conflict", StringComparison.OrdinalIgnoreCase) ? 409 : 400;
            return StatusCode(statusCode, new ProblemDetails { Title = "Appointment update failed", Detail = error, Status = statusCode });
        }
        return Ok(result);
    }

    /// <summary>Update appointment status with workflow enforcement</summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(AppointmentResponseDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] AppointmentStatusUpdateDto dto)
    {
        var (result, error) = await _service.UpdateStatusAsync(id, dto);
        if (error == "Appointment not found.") return NotFound();
        if (error is not null) return BadRequest(new ProblemDetails { Title = "Status update failed", Detail = error, Status = 400 });
        return Ok(result);
    }

    /// <summary>Get all of today's appointments</summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(IEnumerable<AppointmentResponseDto>), 200)]
    public async Task<IActionResult> GetToday()
        => Ok(await _service.GetTodayAsync());
}
