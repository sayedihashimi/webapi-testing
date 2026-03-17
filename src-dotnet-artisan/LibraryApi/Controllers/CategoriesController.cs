using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service) => _service = service;

    /// <summary>List all categories</summary>
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    /// <summary>Get category details with book count</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDetailDto>> GetById(int id)
    {
        var category = await _service.GetByIdAsync(id);
        return category is null ? NotFound() : Ok(category);
    }

    /// <summary>Create a new category</summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryCreateDto dto)
    {
        var category = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>Update a category</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, CategoryUpdateDto dto)
    {
        var category = await _service.UpdateAsync(id, dto);
        return category is null ? NotFound() : Ok(category);
    }

    /// <summary>Delete a category (fails if has books)</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
