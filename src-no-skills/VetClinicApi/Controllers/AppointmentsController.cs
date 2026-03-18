using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Models;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _service;

    public AppointmentsController(IAppointmentService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<AppointmentResponseDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo,
        [FromQuery] AppointmentStatus? status, [FromQuery] int? vetId, [FromQuery] int? petId,
        [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetAllAsync(dateFrom, dateTo, status, vetId, petId, pagination));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AppointmentResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AppointmentResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AppointmentResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(AppointmentResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
        => Ok(await _service.UpdateStatusAsync(id, dto));

    [HttpGet("today")]
    [ProducesResponseType(typeof(List<AppointmentResponseDto>), 200)]
    public async Task<IActionResult> GetToday()
        => Ok(await _service.GetTodayAsync());
}
