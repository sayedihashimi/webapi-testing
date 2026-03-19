using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/instructors")]
[Produces("application/json")]
public class InstructorsController : ControllerBase
{
    private readonly IInstructorService _service;

    public InstructorsController(IInstructorService service)
    {
        _service = service;
    }

    /// <summary>List instructors with optional filtering</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<InstructorResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? specialization, [FromQuery] bool? isActive)
    {
        var result = await _service.GetAllAsync(specialization, isActive);
        return Ok(result);
    }

    /// <summary>Get instructor details</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InstructorResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var instructor = await _service.GetByIdAsync(id);
        return instructor is null ? NotFound() : Ok(instructor);
    }

    /// <summary>Create a new instructor</summary>
    [HttpPost]
    [ProducesResponseType(typeof(InstructorResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] InstructorCreateDto dto)
    {
        var instructor = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = instructor.Id }, instructor);
    }

    /// <summary>Update an instructor</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(InstructorResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int id, [FromBody] InstructorUpdateDto dto)
    {
        var instructor = await _service.UpdateAsync(id, dto);
        return instructor is null ? NotFound() : Ok(instructor);
    }

    /// <summary>Get instructor's class schedule</summary>
    [HttpGet("{id:int}/schedule")]
    [ProducesResponseType(typeof(List<ClassScheduleResponseDto>), 200)]
    public async Task<IActionResult> GetSchedule(int id, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var result = await _service.GetScheduleAsync(id, fromDate, toDate);
        return Ok(result);
    }
}
