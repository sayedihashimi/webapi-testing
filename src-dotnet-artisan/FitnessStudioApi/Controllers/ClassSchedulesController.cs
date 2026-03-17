using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/classes")]
public class ClassSchedulesController(IClassScheduleService service) : ControllerBase
{
    /// <summary>List class schedules with filters and pagination</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ClassScheduleDto>>> GetAll(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int? classTypeId, [FromQuery] int? instructorId,
        [FromQuery] bool? available,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await service.GetAllAsync(from, to, classTypeId, instructorId, available, page, pageSize));

    /// <summary>Get class schedule details</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClassScheduleDto>> GetById(int id)
    {
        var cs = await service.GetByIdAsync(id);
        return cs is null ? NotFound() : Ok(cs);
    }

    /// <summary>Schedule a new class</summary>
    [HttpPost]
    public async Task<ActionResult<ClassScheduleDto>> Create(CreateClassScheduleDto dto)
    {
        var (result, error) = await service.CreateAsync(dto);
        if (result is null) return BadRequest(new { error });
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a class schedule</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClassScheduleDto>> Update(int id, UpdateClassScheduleDto dto)
    {
        var (result, error) = await service.UpdateAsync(id, dto);
        if (result is null) return BadRequest(new { error });
        return Ok(result);
    }

    /// <summary>Cancel a class (cascades to bookings)</summary>
    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancelClassDto dto)
    {
        var (success, error) = await service.CancelAsync(id, dto);
        if (!success) return error == "Class schedule not found." ? NotFound() : BadRequest(new { error });
        return Ok(new { message = "Class cancelled." });
    }

    /// <summary>Get confirmed member roster for a class</summary>
    [HttpGet("{id:int}/roster")]
    public async Task<ActionResult<IReadOnlyList<RosterEntryDto>>> GetRoster(int id)
        => Ok(await service.GetRosterAsync(id));

    /// <summary>Get waitlist for a class</summary>
    [HttpGet("{id:int}/waitlist")]
    public async Task<ActionResult<IReadOnlyList<WaitlistEntryDto>>> GetWaitlist(int id)
        => Ok(await service.GetWaitlistAsync(id));

    /// <summary>Get classes with available spots in the next 7 days</summary>
    [HttpGet("available")]
    public async Task<ActionResult<IReadOnlyList<ClassScheduleDto>>> GetAvailable()
        => Ok(await service.GetAvailableAsync());
}
