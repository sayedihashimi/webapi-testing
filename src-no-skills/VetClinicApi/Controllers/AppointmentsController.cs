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

    /// <summary>Get all appointments with optional filters and pagination</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<AppointmentSummaryDto>>> GetAll(
        [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo,
        [FromQuery] string? status, [FromQuery] int? vetId, [FromQuery] int? petId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        return Ok(await _service.GetAllAsync(dateFrom, dateTo, status, vetId, petId, page, pageSize));
    }

    /// <summary>Get appointment by ID including pet, vet, and medical record</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    /// <summary>Create a new appointment (includes conflict detection)</summary>
    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update an appointment (re-checks for conflicts)</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<AppointmentDto>> Update(int id, [FromBody] UpdateAppointmentDto dto)
    {
        return Ok(await _service.UpdateAsync(id, dto));
    }

    /// <summary>Update appointment status following workflow rules</summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<AppointmentDto>> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
    {
        return Ok(await _service.UpdateStatusAsync(id, dto));
    }

    /// <summary>Get all appointments scheduled for today</summary>
    [HttpGet("today")]
    public async Task<ActionResult<List<AppointmentSummaryDto>>> GetToday()
    {
        return Ok(await _service.GetTodayAsync());
    }
}
