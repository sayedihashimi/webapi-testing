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
    [ProducesResponseType(typeof(PagedResult<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CategoryDto>>> GetCategories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _categoryService.GetCategoriesAsync(new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get category details with book count</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDetailDto>> GetCategory(int id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Create a new category</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetCategory), new { id = result.Id }, result);
    }

    /// <summary>Update an existing category</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, dto);
        return Ok(result);
    }

    /// <summary>Delete a category (fails if category has any books)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        return NoContent();
    }
}
