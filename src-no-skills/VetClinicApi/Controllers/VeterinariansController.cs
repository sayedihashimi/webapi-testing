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

    /// <summary>List all veterinarians with optional filters</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<VetResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? specialization, [FromQuery] bool? isAvailable, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(specialization, isAvailable, new PaginationParams { Page = page, PageSize = pageSize }));

    /// <summary>Get veterinarian details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VetResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var vet = await _service.GetByIdAsync(id);
        return vet is null ? NotFound() : Ok(vet);
    }

    /// <summary>Create a new veterinarian</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VetResponseDto), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] VetCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update veterinarian info</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(VetResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] VetUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Get vet's appointments for a specific date</summary>
    [HttpGet("{id}/schedule")]
    [ProducesResponseType(typeof(IEnumerable<AppointmentResponseDto>), 200)]
    public async Task<IActionResult> GetSchedule(int id, [FromQuery] DateOnly date)
        => Ok(await _service.GetScheduleAsync(id, date));

    /// <summary>Get all appointments for a vet with optional status filter</summary>
    [HttpGet("{id}/appointments")]
    [ProducesResponseType(typeof(PagedResult<AppointmentResponseDto>), 200)]
    public async Task<IActionResult> GetAppointments(int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAppointmentsAsync(id, status, new PaginationParams { Page = page, PageSize = pageSize }));
}
