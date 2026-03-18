using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/classes")]
[Produces("application/json")]
public class ClassSchedulesController : ControllerBase
{
    private readonly IClassScheduleService _service;

    public ClassSchedulesController(IClassScheduleService service) => _service = service;

    /// <summary>List scheduled classes with filters and pagination</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<ClassScheduleDto>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int? classTypeId, [FromQuery] int? instructorId,
        [FromQuery] bool? hasAvailability,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(from, to, classTypeId, instructorId, hasAvailability, page, pageSize));

    /// <summary>Get class details with availability info</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ClassScheduleDetailDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var cs = await _service.GetByIdAsync(id);
        return cs is null ? NotFound() : Ok(cs);
    }

    /// <summary>Schedule a new class</summary>
    [HttpPost]
    [ProducesResponseType<ClassScheduleDto>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateClassScheduleDto dto,
        [FromServices] IValidator<CreateClassScheduleDto> validator)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var schedule = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, schedule);
    }

    /// <summary>Update class schedule details</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<ClassScheduleDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassScheduleDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Cancel a class (cascades to all bookings)</summary>
    [HttpPatch("{id:int}/cancel")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelClassDto dto)
    {
        await _service.CancelAsync(id, dto);
        return NoContent();
    }

    /// <summary>Get confirmed members roster for a class</summary>
    [HttpGet("{id:int}/roster")]
    [ProducesResponseType<IReadOnlyList<RosterEntryDto>>(200)]
    public async Task<IActionResult> GetRoster(int id)
        => Ok(await _service.GetRosterAsync(id));

    /// <summary>Get waitlist for a class</summary>
    [HttpGet("{id:int}/waitlist")]
    [ProducesResponseType<IReadOnlyList<WaitlistEntryDto>>(200)]
    public async Task<IActionResult> GetWaitlist(int id)
        => Ok(await _service.GetWaitlistAsync(id));

    /// <summary>Get classes with available spots in the next 7 days</summary>
    [HttpGet("available")]
    [ProducesResponseType<IReadOnlyList<ClassScheduleDto>>(200)]
    public async Task<IActionResult> GetAvailable()
        => Ok(await _service.GetAvailableAsync());
}
