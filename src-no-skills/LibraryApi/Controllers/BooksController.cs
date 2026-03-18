using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>List books with search, filter by availability, pagination, and sorting</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<BookDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? available,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _bookService.GetAllAsync(search, category, available, sortBy, sortOrder, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get book details including authors, categories, and availability info</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book == null) return NotFound(new ProblemDetails { Title = "Book not found", Status = 404 });
        return Ok(book);
    }

    /// <summary>Create a new book (accepts author IDs and category IDs)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookDto dto)
    {
        var book = await _bookService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    /// <summary>Update an existing book</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookDto dto)
    {
        var book = await _bookService.UpdateAsync(id, dto);
        if (book == null) return NotFound(new ProblemDetails { Title = "Book not found", Status = 404 });
        return Ok(book);
    }

    /// <summary>Delete a book (fails if the book has any active loans)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await _bookService.DeleteAsync(id);
        if (!success)
        {
            if (error == "Book not found")
                return NotFound(new ProblemDetails { Title = error, Status = 404 });
            return Conflict(new ProblemDetails { Title = "Delete failed", Detail = error, Status = 409 });
        }
        return NoContent();
    }

    /// <summary>Get the loan history for a specific book</summary>
    [HttpGet("{id}/loans")]
    [ProducesResponseType(typeof(PaginatedResponse<LoanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookLoans(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _bookService.GetBookLoansAsync(id, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get the active reservations queue for a specific book</summary>
    [HttpGet("{id}/reservations")]
    [ProducesResponseType(typeof(PaginatedResponse<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookReservations(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _bookService.GetBookReservationsAsync(id, page, pageSize);
        return Ok(result);
    }
}
