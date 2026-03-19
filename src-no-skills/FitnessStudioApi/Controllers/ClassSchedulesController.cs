using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/classes")]
[Produces("application/json")]
public class ClassSchedulesController : ControllerBase
{
    private readonly IClassScheduleService _service;

    public ClassSchedulesController(IClassScheduleService service)
    {
        _service = service;
    }

    /// <summary>List scheduled classes with filtering and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClassScheduleResponseDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
        [FromQuery] int? classTypeId, [FromQuery] int? instructorId,
        [FromQuery] bool? hasAvailability,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetAllAsync(fromDate, toDate, classTypeId, instructorId, hasAvailability,
            new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get class schedule details</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClassScheduleResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var classSchedule = await _service.GetByIdAsync(id);
        return classSchedule is null ? NotFound() : Ok(classSchedule);
    }

    /// <summary>Schedule a new class</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClassScheduleResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] ClassScheduleCreateDto dto)
    {
        var classSchedule = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = classSchedule.Id }, classSchedule);
    }

    /// <summary>Update class details</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ClassScheduleResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int id, [FromBody] ClassScheduleUpdateDto dto)
    {
        var classSchedule = await _service.UpdateAsync(id, dto);
        return classSchedule is null ? NotFound() : Ok(classSchedule);
    }

    /// <summary>Cancel a class (cascade cancels all bookings)</summary>
    [HttpPatch("{id:int}/cancel")]
    [ProducesResponseType(typeof(ClassScheduleResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelClassDto dto)
    {
        var result = await _service.CancelAsync(id, dto);
        return Ok(result);
    }

    /// <summary>Get the roster of confirmed members for a class</summary>
    [HttpGet("{id:int}/roster")]
    [ProducesResponseType(typeof(List<RosterEntryDto>), 200)]
    public async Task<IActionResult> GetRoster(int id)
    {
        var result = await _service.GetRosterAsync(id);
        return Ok(result);
    }

    /// <summary>Get the waitlist for a class</summary>
    [HttpGet("{id:int}/waitlist")]
    [ProducesResponseType(typeof(List<WaitlistEntryDto>), 200)]
    public async Task<IActionResult> GetWaitlist(int id)
    {
        var result = await _service.GetWaitlistAsync(id);
        return Ok(result);
    }

    /// <summary>Get classes with available spots in the next 7 days</summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(List<ClassScheduleResponseDto>), 200)]
    public async Task<IActionResult> GetAvailable()
    {
        var result = await _service.GetAvailableAsync();
        return Ok(result);
    }
}
