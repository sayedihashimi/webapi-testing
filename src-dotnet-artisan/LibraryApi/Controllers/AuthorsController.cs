using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _service;

    public AuthorsController(IAuthorService service) => _service = service;

    /// <summary>List authors with optional search and pagination</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<AuthorDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;
        return Ok(await _service.GetAllAsync(search, page, pageSize));
    }

    /// <summary>Get author details with books</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AuthorDetailDto>> GetById(int id)
    {
        var author = await _service.GetByIdAsync(id);
        return author is null ? NotFound() : Ok(author);
    }

    /// <summary>Create a new author</summary>
    [HttpPost]
    public async Task<ActionResult<AuthorDto>> Create(AuthorCreateDto dto)
    {
        var author = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
    }

    /// <summary>Update an author</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<AuthorDto>> Update(int id, AuthorUpdateDto dto)
    {
        var author = await _service.UpdateAsync(id, dto);
        return author is null ? NotFound() : Ok(author);
    }

    /// <summary>Delete an author (fails if has books)</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
