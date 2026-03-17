using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/instructors")]
public class InstructorsController(IInstructorService service) : ControllerBase
{
    /// <summary>List instructors with filters</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InstructorDto>>> GetAll(
        [FromQuery] string? specialization, [FromQuery] bool? isActive)
        => Ok(await service.GetAllAsync(specialization, isActive));

    /// <summary>Get instructor details</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<InstructorDto>> GetById(int id)
    {
        var instructor = await service.GetByIdAsync(id);
        return instructor is null ? NotFound() : Ok(instructor);
    }

    /// <summary>Create a new instructor</summary>
    [HttpPost]
    public async Task<ActionResult<InstructorDto>> Create(CreateInstructorDto dto)
    {
        var instructor = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = instructor.Id }, instructor);
    }

    /// <summary>Update an instructor</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<InstructorDto>> Update(int id, UpdateInstructorDto dto)
    {
        var instructor = await service.UpdateAsync(id, dto);
        return instructor is null ? NotFound() : Ok(instructor);
    }

    /// <summary>Get instructor schedule with date range filter</summary>
    [HttpGet("{id:int}/schedule")]
    public async Task<ActionResult<IReadOnlyList<ClassScheduleDto>>> GetSchedule(
        int id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await service.GetScheduleAsync(id, from, to));
}
