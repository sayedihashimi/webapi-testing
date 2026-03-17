using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VeterinariansController : ControllerBase
{
    private readonly IVeterinarianService _service;

    public VeterinariansController(IVeterinarianService service) => _service = service;

    /// <summary>Get all veterinarians with optional filters</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<VeterinarianDto>>> GetAll(
        [FromQuery] string? specialization, [FromQuery] bool? isAvailable,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        return Ok(await _service.GetAllAsync(specialization, isAvailable, page, pageSize));
    }

    /// <summary>Get veterinarian by ID</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VeterinarianDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    /// <summary>Create a new veterinarian</summary>
    [HttpPost]
    public async Task<ActionResult<VeterinarianDto>> Create([FromBody] CreateVeterinarianDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a veterinarian</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<VeterinarianDto>> Update(int id, [FromBody] UpdateVeterinarianDto dto)
    {
        return Ok(await _service.UpdateAsync(id, dto));
    }

    /// <summary>Get veterinarian schedule for a specific date</summary>
    [HttpGet("{id}/schedule")]
    public async Task<ActionResult<List<AppointmentSummaryDto>>> GetSchedule(int id, [FromQuery] DateOnly date)
    {
        return Ok(await _service.GetScheduleAsync(id, date));
    }

    /// <summary>Get veterinarian's appointments with pagination and optional status filter</summary>
    [HttpGet("{id}/appointments")]
    public async Task<ActionResult<PaginatedResponse<AppointmentSummaryDto>>> GetAppointments(
        int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        return Ok(await _service.GetAppointmentsAsync(id, status, page, pageSize));
    }
}
