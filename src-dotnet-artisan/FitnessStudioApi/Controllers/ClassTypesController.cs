using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/class-types")]
public class ClassTypesController(IClassTypeService service) : ControllerBase
{
    /// <summary>List class types with filters</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClassTypeDto>>> GetAll(
        [FromQuery] DifficultyLevel? difficulty, [FromQuery] bool? isPremium)
        => Ok(await service.GetAllAsync(difficulty, isPremium));

    /// <summary>Get class type details</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClassTypeDto>> GetById(int id)
    {
        var ct = await service.GetByIdAsync(id);
        return ct is null ? NotFound() : Ok(ct);
    }

    /// <summary>Create a new class type</summary>
    [HttpPost]
    public async Task<ActionResult<ClassTypeDto>> Create(CreateClassTypeDto dto)
    {
        var ct = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = ct.Id }, ct);
    }

    /// <summary>Update a class type</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClassTypeDto>> Update(int id, UpdateClassTypeDto dto)
    {
        var ct = await service.UpdateAsync(id, dto);
        return ct is null ? NotFound() : Ok(ct);
    }
}
