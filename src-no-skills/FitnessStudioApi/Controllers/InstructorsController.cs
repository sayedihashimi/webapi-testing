using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/instructors")]
[Produces("application/json")]
public class InstructorsController : ControllerBase
{
    private readonly IInstructorService _service;

    public InstructorsController(IInstructorService service) => _service = service;

    /// <summary>List instructors with optional filters</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InstructorDto>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? specialization, [FromQuery] bool? isActive)
        => Ok(await _service.GetAllAsync(specialization, isActive));

    /// <summary>Get instructor details</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<InstructorDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var instructor = await _service.GetByIdAsync(id);
        return instructor is null ? NotFound() : Ok(instructor);
    }

    /// <summary>Create an instructor</summary>
    [HttpPost]
    [ProducesResponseType<InstructorDto>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateInstructorDto dto,
        [FromServices] IValidator<CreateInstructorDto> validator)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var instructor = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = instructor.Id }, instructor);
    }

    /// <summary>Update an instructor</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<InstructorDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInstructorDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Get instructor's class schedule</summary>
    [HttpGet("{id:int}/schedule")]
    [ProducesResponseType<IReadOnlyList<ClassScheduleDto>>(200)]
    public async Task<IActionResult> GetSchedule(
        int id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await _service.GetScheduleAsync(id, from, to));
}
