using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/classes")]
[Produces("application/json")]
public class ClassSchedulesController : ControllerBase
{
    private readonly ClassScheduleService _service;

    public ClassSchedulesController(ClassScheduleService service)
    {
        _service = service;
    }

    /// <summary>Get all class schedules with filtering and pagination</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ClassScheduleDto>>> GetAll(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int? classTypeId, [FromQuery] int? instructorId,
        [FromQuery] bool? hasAvailability,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(from, to, classTypeId, instructorId, hasAvailability, page, pageSize));

    /// <summary>Get a class schedule by ID with enrollment details</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ClassScheduleDto>> GetById(int id)
    {
        var cs = await _service.GetByIdAsync(id);
        return cs == null ? NotFound() : Ok(cs);
    }

    /// <summary>Create a new class schedule</summary>
    [HttpPost]
    public async Task<ActionResult<ClassScheduleDto>> Create(CreateClassScheduleDto dto)
    {
        var cs = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = cs.Id }, cs);
    }

    /// <summary>Update a class schedule</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ClassScheduleDto>> Update(int id, UpdateClassScheduleDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Cancel a class and cascade-cancel all bookings</summary>
    [HttpPatch("{id}/cancel")]
    public async Task<ActionResult<ClassScheduleDto>> Cancel(int id, CancelClassDto dto)
        => Ok(await _service.CancelClassAsync(id, dto));

    /// <summary>Get the roster for a class</summary>
    [HttpGet("{id}/roster")]
    public async Task<ActionResult<List<ClassRosterEntryDto>>> GetRoster(int id)
        => Ok(await _service.GetRosterAsync(id));

    /// <summary>Get the waitlist for a class</summary>
    [HttpGet("{id}/waitlist")]
    public async Task<ActionResult<List<ClassWaitlistEntryDto>>> GetWaitlist(int id)
        => Ok(await _service.GetWaitlistAsync(id));

    /// <summary>Get all available classes (scheduled with open spots)</summary>
    [HttpGet("available")]
    public async Task<ActionResult<List<ClassScheduleDto>>> GetAvailable()
        => Ok(await _service.GetAvailableAsync());
}
