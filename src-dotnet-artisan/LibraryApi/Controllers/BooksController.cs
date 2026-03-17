using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _service;

    public BooksController(IBookService service) => _service = service;

    /// <summary>List books with search, availability filter, pagination, sorting</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<BookDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? available,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;
        return Ok(await _service.GetAllAsync(search, available, sortBy, sortDir, page, pageSize));
    }

    /// <summary>Get book details with authors, categories, availability</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BookDetailDto>> GetById(int id)
    {
        var book = await _service.GetByIdAsync(id);
        return book is null ? NotFound() : Ok(book);
    }

    /// <summary>Create a new book with author and category IDs</summary>
    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(BookCreateDto dto)
    {
        var book = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    /// <summary>Update a book</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<BookDto>> Update(int id, BookUpdateDto dto)
    {
        var book = await _service.UpdateAsync(id, dto);
        return book is null ? NotFound() : Ok(book);
    }

    /// <summary>Delete a book (fails if active loans)</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get loan history for a book</summary>
    [HttpGet("{id}/loans")]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetLoans(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;
        return Ok(await _service.GetLoansAsync(id, page, pageSize));
    }

    /// <summary>Get active reservation queue for a book</summary>
    [HttpGet("{id}/reservations")]
    public async Task<ActionResult<List<ReservationDto>>> GetReservations(int id)
    {
        return Ok(await _service.GetReservationsAsync(id));
    }
}
