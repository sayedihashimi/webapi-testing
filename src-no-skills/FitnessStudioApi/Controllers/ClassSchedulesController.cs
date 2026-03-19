using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

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
    [ProducesResponseType(typeof(PagedResult<ClassScheduleDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
        [FromQuery] int? classTypeId, [FromQuery] int? instructorId,
        [FromQuery] bool? hasAvailability, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetAllAsync(fromDate, toDate, classTypeId, instructorId, hasAvailability, pagination));

    /// <summary>Get class details including roster and waitlist counts</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClassScheduleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    /// <summary>Schedule a new class</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClassScheduleDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateClassScheduleDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update class details</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClassScheduleDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassScheduleDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Cancel a class (cascade cancels all bookings)</summary>
    [HttpPatch("{id}/cancel")]
    [ProducesResponseType(typeof(ClassScheduleDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelClassDto dto)
        => Ok(await _service.CancelAsync(id, dto));

    /// <summary>Get the list of confirmed members for a class</summary>
    [HttpGet("{id}/roster")]
    [ProducesResponseType(typeof(List<BookingDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRoster(int id) => Ok(await _service.GetRosterAsync(id));

    /// <summary>Get the waitlist for a class</summary>
    [HttpGet("{id}/waitlist")]
    [ProducesResponseType(typeof(List<BookingDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetWaitlist(int id) => Ok(await _service.GetWaitlistAsync(id));

    /// <summary>Get classes with available spots in the next 7 days</summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(List<ClassScheduleDto>), 200)]
    public async Task<IActionResult> GetAvailable() => Ok(await _service.GetAvailableAsync());
}
