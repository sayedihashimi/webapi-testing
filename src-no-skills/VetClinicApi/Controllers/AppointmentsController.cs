using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs.Appointment;
using VetClinicApi.DTOs.Common;
using VetClinicApi.Models.Enums;
using VetClinicApi.Services.Interfaces;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService) => _appointmentService = appointmentService;

    /// <summary>List appointments (filter by date, status, vet, pet; pagination)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateOnly? date,
        [FromQuery] AppointmentStatus? status,
        [FromQuery] int? vetId,
        [FromQuery] int? petId,
        [FromQuery] PaginationParams? pagination = null)
        => Ok(await _appointmentService.GetAllAsync(date, status, vetId, petId, pagination ?? new PaginationParams()));

    /// <summary>Get appointment details with pet, vet, and medical record</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AppointmentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _appointmentService.GetByIdAsync(id));

    /// <summary>Schedule a new appointment (enforces conflicts)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
    {
        var result = await _appointmentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update an appointment (re-check conflicts)</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentDto dto)
        => Ok(await _appointmentService.UpdateAsync(id, dto));

    /// <summary>Update appointment status (enforce workflow transitions)</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
        => Ok(await _appointmentService.UpdateStatusAsync(id, dto));

    /// <summary>Get today's appointments</summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(List<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetToday()
        => Ok(await _appointmentService.GetTodayAsync());
}
