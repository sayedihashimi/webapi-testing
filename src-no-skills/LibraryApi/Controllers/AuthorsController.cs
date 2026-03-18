using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorsController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    /// <summary>List authors with search by name and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<AuthorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _authorService.GetAllAsync(search, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get author details including their books</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuthorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var author = await _authorService.GetByIdAsync(id);
        if (author == null) return NotFound(new ProblemDetails { Title = "Author not found", Status = 404 });
        return Ok(author);
    }

    /// <summary>Create a new author</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAuthorDto dto)
    {
        var author = await _authorService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
    }

    /// <summary>Update an existing author</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAuthorDto dto)
    {
        var author = await _authorService.UpdateAsync(id, dto);
        if (author == null) return NotFound(new ProblemDetails { Title = "Author not found", Status = 404 });
        return Ok(author);
    }

    /// <summary>Delete an author (fails if the author has any books)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await _authorService.DeleteAsync(id);
        if (!success)
        {
            if (error == "Author not found")
                return NotFound(new ProblemDetails { Title = error, Status = 404 });
            return Conflict(new ProblemDetails { Title = "Delete failed", Detail = error, Status = 409 });
        }
        return NoContent();
    }
}
