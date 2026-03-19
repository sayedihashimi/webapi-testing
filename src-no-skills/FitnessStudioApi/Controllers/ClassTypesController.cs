using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

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
    [ProducesResponseType(typeof(List<ClassTypeDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? difficulty, [FromQuery] bool? isPremium)
        => Ok(await _service.GetAllAsync(difficulty, isPremium));

    /// <summary>Get class type details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClassTypeDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    /// <summary>Create a new class type</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClassTypeDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateClassTypeDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a class type</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClassTypeDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassTypeDto dto)
        => Ok(await _service.UpdateAsync(id, dto));
}
