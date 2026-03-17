using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BookService _service;

    public BooksController(BookService service) => _service = service;

    /// <summary>Get all books with search, filter, pagination, and sorting.</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<BookSummaryDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? category,
        [FromQuery] bool? available, [FromQuery] string? sortBy,
        [FromQuery] string? sortDir, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(search, category, available, sortBy, sortDir, page, pageSize));

    /// <summary>Get book by ID with authors, categories, and availability.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BookDetailDto>> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Create a new book with author and category IDs.</summary>
    [HttpPost]
    public async Task<ActionResult<BookDetailDto>> Create(CreateBookDto dto)
    {
        var book = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    /// <summary>Update an existing book.</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<BookDetailDto>> Update(int id, UpdateBookDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Delete a book (fails if active loans).</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get loans for a specific book.</summary>
    [HttpGet("{id}/loans")]
    public async Task<ActionResult<PaginatedResponse<LoanDto>>> GetLoans(
        int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetBookLoansAsync(id, page, pageSize));

    /// <summary>Get reservations for a specific book.</summary>
    [HttpGet("{id}/reservations")]
    public async Task<ActionResult<PaginatedResponse<ReservationDto>>> GetReservations(
        int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetBookReservationsAsync(id, page, pageSize));
}
