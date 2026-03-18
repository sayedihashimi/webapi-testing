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
    private readonly IClassTypeService _service;

    public ClassTypesController(IClassTypeService service)
    {
        _service = service;
    }

    /// <summary>List class types with optional filters</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClassTypeDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DifficultyLevel? difficulty,
        [FromQuery] bool? isPremium)
    {
        var types = await _service.GetAllAsync(difficulty, isPremium);
        return Ok(types);
    }

    /// <summary>Get class type details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClassTypeDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var type = await _service.GetByIdAsync(id);
        return Ok(type);
    }

    /// <summary>Create a new class type</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClassTypeDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateClassTypeDto dto)
    {
        var type = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = type.Id }, type);
    }

    /// <summary>Update a class type</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClassTypeDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassTypeDto dto)
    {
        var type = await _service.UpdateAsync(id, dto);
        return Ok(type);
    }
}
