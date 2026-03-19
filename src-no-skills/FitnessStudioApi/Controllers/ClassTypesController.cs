using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

    /// <summary>List class types with optional filtering</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClassTypeResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? difficulty, [FromQuery] bool? isPremium)
    {
        var result = await _service.GetAllAsync(difficulty, isPremium);
        return Ok(result);
    }

    /// <summary>Get class type details</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClassTypeResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var classType = await _service.GetByIdAsync(id);
        return classType is null ? NotFound() : Ok(classType);
    }

    /// <summary>Create a new class type</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClassTypeResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] ClassTypeCreateDto dto)
    {
        var classType = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = classType.Id }, classType);
    }

    /// <summary>Update a class type</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ClassTypeResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int id, [FromBody] ClassTypeUpdateDto dto)
    {
        var classType = await _service.UpdateAsync(id, dto);
        return classType is null ? NotFound() : Ok(classType);
    }
}
