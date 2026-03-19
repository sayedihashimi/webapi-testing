using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _service;

    public AuthorsController(IAuthorService service) => _service = service;

    /// <summary>List authors with optional name search and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuthorDto>), 200)]
    public async Task<IActionResult> GetAuthors([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAuthorsAsync(search, page, pageSize));

    /// <summary>Get author details including their books</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuthorDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAuthor(int id)
        => Ok(await _service.GetAuthorByIdAsync(id));

    /// <summary>Create a new author</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AuthorDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateAuthor([FromBody] AuthorCreateDto dto)
    {
        var author = await _service.CreateAuthorAsync(dto);
        return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, author);
    }

    /// <summary>Update an existing author</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AuthorDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateAuthor(int id, [FromBody] AuthorUpdateDto dto)
        => Ok(await _service.UpdateAuthorAsync(id, dto));

    /// <summary>Delete an author (fails if author has books)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        await _service.DeleteAuthorAsync(id);
        return NoContent();
    }
}
