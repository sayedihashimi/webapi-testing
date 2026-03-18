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

    /// <summary>List authors with optional name search and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuthorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AuthorDto>>> GetAuthors(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _authorService.GetAuthorsAsync(search, new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get author details including their books</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuthorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthorDetailDto>> GetAuthor(int id)
    {
        var result = await _authorService.GetAuthorByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Create a new author</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthorDto>> CreateAuthor([FromBody] CreateAuthorDto dto)
    {
        var result = await _authorService.CreateAuthorAsync(dto);
        return CreatedAtAction(nameof(GetAuthor), new { id = result.Id }, result);
    }

    /// <summary>Update an existing author</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthorDto>> UpdateAuthor(int id, [FromBody] UpdateAuthorDto dto)
    {
        var result = await _authorService.UpdateAuthorAsync(id, dto);
        return Ok(result);
    }

    /// <summary>Delete an author (fails if the author has any books)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        await _authorService.DeleteAuthorAsync(id);
        return NoContent();
    }
}
