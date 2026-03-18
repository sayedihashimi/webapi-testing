using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/class-types")]
[Produces("application/json")]
public class ClassTypesController : ControllerBase
{
    private readonly IClassTypeService _service;

    public ClassTypesController(IClassTypeService service) => _service = service;

    /// <summary>List class types with optional filters</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClassTypeDto>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? difficulty, [FromQuery] bool? isPremium)
        => Ok(await _service.GetAllAsync(difficulty, isPremium));

    /// <summary>Get class type details</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ClassTypeDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var ct = await _service.GetByIdAsync(id);
        return ct is null ? NotFound() : Ok(ct);
    }

    /// <summary>Create a class type</summary>
    [HttpPost]
    [ProducesResponseType<ClassTypeDto>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateClassTypeDto dto,
        [FromServices] IValidator<CreateClassTypeDto> validator)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var ct = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = ct.Id }, ct);
    }

    /// <summary>Update a class type</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<ClassTypeDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassTypeDto dto)
        => Ok(await _service.UpdateAsync(id, dto));
}
