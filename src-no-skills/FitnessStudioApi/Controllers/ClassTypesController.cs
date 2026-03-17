using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/class-types")]
[Produces("application/json")]
public class ClassTypesController : ControllerBase
{
    private readonly ClassTypeService _service;

    public ClassTypesController(ClassTypeService service)
    {
        _service = service;
    }

    /// <summary>Get all class types with optional filters</summary>
    [HttpGet]
    public async Task<ActionResult<List<ClassTypeDto>>> GetAll(
        [FromQuery] DifficultyLevel? difficulty, [FromQuery] bool? isPremium)
        => Ok(await _service.GetAllAsync(difficulty, isPremium));

    /// <summary>Get a class type by ID</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ClassTypeDto>> GetById(int id)
    {
        var ct = await _service.GetByIdAsync(id);
        return ct == null ? NotFound() : Ok(ct);
    }

    /// <summary>Create a new class type</summary>
    [HttpPost]
    public async Task<ActionResult<ClassTypeDto>> Create(CreateClassTypeDto dto)
    {
        var ct = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = ct.Id }, ct);
    }

    /// <summary>Update a class type</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ClassTypeDto>> Update(int id, UpdateClassTypeDto dto)
        => Ok(await _service.UpdateAsync(id, dto));
}
