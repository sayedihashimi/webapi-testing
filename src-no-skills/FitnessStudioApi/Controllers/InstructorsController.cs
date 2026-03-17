using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/instructors")]
[Produces("application/json")]
public class InstructorsController : ControllerBase
{
    private readonly InstructorService _service;

    public InstructorsController(InstructorService service)
    {
        _service = service;
    }

    /// <summary>Get all instructors with optional filters</summary>
    [HttpGet]
    public async Task<ActionResult<List<InstructorDto>>> GetAll(
        [FromQuery] string? specialization, [FromQuery] bool? isActive)
        => Ok(await _service.GetAllAsync(specialization, isActive));

    /// <summary>Get an instructor by ID</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InstructorDto>> GetById(int id)
    {
        var instructor = await _service.GetByIdAsync(id);
        return instructor == null ? NotFound() : Ok(instructor);
    }

    /// <summary>Create a new instructor</summary>
    [HttpPost]
    public async Task<ActionResult<InstructorDto>> Create(CreateInstructorDto dto)
    {
        var instructor = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = instructor.Id }, instructor);
    }

    /// <summary>Update an instructor</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<InstructorDto>> Update(int id, UpdateInstructorDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Get an instructor's schedule</summary>
    [HttpGet("{id}/schedule")]
    public async Task<ActionResult<List<ClassScheduleDto>>> GetSchedule(
        int id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await _service.GetScheduleAsync(id, from, to));
}
