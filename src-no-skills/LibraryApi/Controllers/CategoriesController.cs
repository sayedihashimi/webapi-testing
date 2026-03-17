using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _service;

    public CategoriesController(CategoryService service) => _service = service;

    /// <summary>Get all categories.</summary>
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    /// <summary>Get category by ID with book count.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDetailDto>> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Create a new category.</summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
    {
        var category = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>Update an existing category.</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, UpdateCategoryDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Delete a category (fails if has books).</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
