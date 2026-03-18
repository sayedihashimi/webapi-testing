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

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<VeterinarianResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? specialization, [FromQuery] bool? isAvailable, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetAllAsync(specialization, isAvailable, pagination));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VeterinarianResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(VeterinarianResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateVeterinarianDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(VeterinarianResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVeterinarianDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    [HttpGet("{id}/schedule")]
    [ProducesResponseType(typeof(List<AppointmentResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSchedule(int id, [FromQuery] DateOnly date)
        => Ok(await _service.GetScheduleAsync(id, date));

    [HttpGet("{id}/appointments")]
    [ProducesResponseType(typeof(PagedResponse<AppointmentResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAppointments(int id, [FromQuery] string? status, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetAppointmentsAsync(id, status, pagination));
}
