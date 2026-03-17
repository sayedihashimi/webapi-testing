using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly AuthorService _service;

    public AuthorsController(AuthorService service) => _service = service;

    /// <summary>Get all authors with optional name search and pagination.</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<AuthorDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(search, page, pageSize));

    /// <summary>Get author by ID with associated books.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AuthorDetailDto>> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Create a new author.</summary>
    [HttpPost]
    public async Task<ActionResult<AuthorDto>> Create(CreateAuthorDto dto)
    {
        var author = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
    }

    /// <summary>Update an existing author.</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<AuthorDto>> Update(int id, UpdateAuthorDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Delete an author (fails if author has books).</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
