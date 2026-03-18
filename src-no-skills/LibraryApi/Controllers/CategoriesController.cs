using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>List all categories with pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _categoryService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Get category details with count of books</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null) return NotFound(new ProblemDetails { Title = "Category not found", Status = 404 });
        return Ok(category);
    }

    /// <summary>Create a new category</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var category = await _categoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>Update an existing category</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        var category = await _categoryService.UpdateAsync(id, dto);
        if (category == null) return NotFound(new ProblemDetails { Title = "Category not found", Status = 404 });
        return Ok(category);
    }

    /// <summary>Delete a category (fails if category has any books)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await _categoryService.DeleteAsync(id);
        if (!success)
        {
            if (error == "Category not found")
                return NotFound(new ProblemDetails { Title = error, Status = 404 });
            return Conflict(new ProblemDetails { Title = "Delete failed", Detail = error, Status = 409 });
        }
        return NoContent();
    }
}
