using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/classes")]
[Produces("application/json")]
public class ClassesController : ControllerBase
{
    private readonly IClassScheduleService _service;

    public ClassesController(IClassScheduleService service)
    {
        _service = service;
    }

    /// <summary>List scheduled classes with filters and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClassScheduleDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? classTypeId,
        [FromQuery] int? instructorId,
        [FromQuery] bool? available,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(from, to, classTypeId, instructorId, available, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get class details including roster count, waitlist count, and availability</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClassScheduleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var schedule = await _service.GetByIdAsync(id);
        return Ok(schedule);
    }

    /// <summary>Schedule a new class</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClassScheduleDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateClassScheduleDto dto)
    {
        var schedule = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, schedule);
    }

    /// <summary>Update class details</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClassScheduleDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassScheduleDto dto)
    {
        var schedule = await _service.UpdateAsync(id, dto);
        return Ok(schedule);
    }

    /// <summary>Cancel a class (cascades to all bookings)</summary>
    [HttpPatch("{id}/cancel")]
    [ProducesResponseType(typeof(ClassScheduleDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelClassDto dto)
    {
        var schedule = await _service.CancelAsync(id, dto);
        return Ok(schedule);
    }

    /// <summary>Get the list of confirmed members for a class</summary>
    [HttpGet("{id}/roster")]
    [ProducesResponseType(typeof(List<ClassRosterItemDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRoster(int id)
    {
        var roster = await _service.GetRosterAsync(id);
        return Ok(roster);
    }

    /// <summary>Get the waitlist for a class</summary>
    [HttpGet("{id}/waitlist")]
    [ProducesResponseType(typeof(List<ClassRosterItemDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetWaitlist(int id)
    {
        var waitlist = await _service.GetWaitlistAsync(id);
        return Ok(waitlist);
    }

    /// <summary>Get classes with available spots in the next 7 days</summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(List<ClassScheduleDto>), 200)]
    public async Task<IActionResult> GetAvailable()
    {
        var available = await _service.GetAvailableAsync();
        return Ok(available);
    }
}
