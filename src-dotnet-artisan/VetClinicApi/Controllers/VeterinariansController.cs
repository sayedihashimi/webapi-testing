using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VeterinariansController(IVeterinarianService vetService) : ControllerBase
{
    /// <summary>List veterinarians with optional filters</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<VeterinarianDto>>> GetAll(
        [FromQuery] string? specialization,
        [FromQuery] bool? isAvailable,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await vetService.GetAllAsync(specialization, isAvailable, new PaginationParams(page, pageSize));
        return Ok(result);
    }

    /// <summary>Get veterinarian details</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VeterinarianDto>> GetById(int id)
    {
        var vet = await vetService.GetByIdAsync(id);
        return vet is null ? NotFound() : Ok(vet);
    }

    /// <summary>Register a new veterinarian</summary>
    [HttpPost]
    public async Task<ActionResult<VeterinarianDto>> Create([FromBody] CreateVeterinarianDto dto)
    {
        var vet = await vetService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = vet.Id }, vet);
    }

    /// <summary>Update veterinarian info</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<VeterinarianDto>> Update(int id, [FromBody] UpdateVeterinarianDto dto)
    {
        var vet = await vetService.UpdateAsync(id, dto);
        return vet is null ? NotFound() : Ok(vet);
    }

    /// <summary>Get vet's schedule for a specific date</summary>
    [HttpGet("{id:int}/schedule")]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetSchedule(int id, [FromQuery] DateOnly date)
    {
        var schedule = await vetService.GetScheduleAsync(id, date);
        return Ok(schedule);
    }

    /// <summary>Get all appointments for a veterinarian</summary>
    [HttpGet("{id:int}/appointments")]
    public async Task<ActionResult<PagedResult<AppointmentDto>>> GetAppointments(
        int id,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await vetService.GetAppointmentsAsync(id, status, new PaginationParams(page, pageSize));
        return Ok(result);
    }
}
