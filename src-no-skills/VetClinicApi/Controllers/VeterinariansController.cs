using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs.Common;
using VetClinicApi.DTOs.Veterinarian;
using VetClinicApi.Services.Interfaces;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VeterinariansController : ControllerBase
{
    private readonly IVeterinarianService _vetService;

    public VeterinariansController(IVeterinarianService vetService) => _vetService = vetService;

    /// <summary>List veterinarians (filter by specialization, availability)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<VeterinarianDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? specialization, [FromQuery] bool? isAvailable, [FromQuery] PaginationParams? pagination = null)
        => Ok(await _vetService.GetAllAsync(specialization, isAvailable, pagination ?? new PaginationParams()));

    /// <summary>Get veterinarian details</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VeterinarianDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _vetService.GetByIdAsync(id));

    /// <summary>Create a new veterinarian</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VeterinarianDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateVeterinarianDto dto)
    {
        var result = await _vetService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a veterinarian</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(VeterinarianDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVeterinarianDto dto)
        => Ok(await _vetService.UpdateAsync(id, dto));

    /// <summary>Get veterinarian's schedule for a specific date</summary>
    [HttpGet("{id:int}/schedule")]
    [ProducesResponseType(typeof(List<DTOs.Appointment.AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchedule(int id, [FromQuery] DateOnly date)
        => Ok(await _vetService.GetScheduleAsync(id, date));

    /// <summary>Get veterinarian's appointments</summary>
    [HttpGet("{id:int}/appointments")]
    [ProducesResponseType(typeof(PagedResult<DTOs.Appointment.AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointments(int id, [FromQuery] PaginationParams? pagination = null)
        => Ok(await _vetService.GetAppointmentsAsync(id, pagination ?? new PaginationParams()));
}
