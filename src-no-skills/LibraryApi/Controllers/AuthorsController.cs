using LibraryApi.DTOs.Author;
using LibraryApi.DTOs.Common;
using LibraryApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _service;

    public AuthorsController(IAuthorService service) => _service = service;

    /// <summary>List authors with search and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuthorListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetAllAsync(search, pagination));

    /// <summary>Get author details with books.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuthorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Create a new author.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AuthorDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAuthorDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update an author.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AuthorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAuthorDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Delete an author (fails if has books).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
