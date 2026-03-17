using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Models;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    /// <summary>List appointments with optional filters</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<AppointmentDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] AppointmentStatus? status,
        [FromQuery] int? vetId,
        [FromQuery] int? petId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await appointmentService.GetAllAsync(fromDate, toDate, status, vetId, petId, new PaginationParams(page, pageSize));
        return Ok(result);
    }

    /// <summary>Get appointment details with pet, vet, and medical record</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentDetailDto>> GetById(int id)
    {
        var appointment = await appointmentService.GetByIdAsync(id);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    /// <summary>Schedule a new appointment (enforces conflict detection)</summary>
    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentDto dto)
    {
        var appointment = await appointmentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    /// <summary>Update an appointment (re-checks conflicts if date changed)</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AppointmentDto>> Update(int id, [FromBody] UpdateAppointmentDto dto)
    {
        var appointment = await appointmentService.UpdateAsync(id, dto);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    /// <summary>Update appointment status (enforces workflow transitions)</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<AppointmentDto>> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
    {
        var appointment = await appointmentService.UpdateStatusAsync(id, dto);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    /// <summary>Get today's appointments</summary>
    [HttpGet("today")]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetToday()
    {
        var appointments = await appointmentService.GetTodayAsync();
        return Ok(appointments);
    }
}
