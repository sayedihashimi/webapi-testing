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

    /// <summary>List books with search, filter by availability, pagination and sorting</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<BookDto>>> GetBooks(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? available,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _bookService.GetBooksAsync(search, category, available, sortBy, sortOrder,
            new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get book details including authors, categories, and availability</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDto>> GetBook(int id)
    {
        var result = await _bookService.GetBookByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Create a new book with author IDs and category IDs</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookDto>> CreateBook([FromBody] CreateBookDto dto)
    {
        var result = await _bookService.CreateBookAsync(dto);
        return CreatedAtAction(nameof(GetBook), new { id = result.Id }, result);
    }

    /// <summary>Update an existing book</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookDto>> UpdateBook(int id, [FromBody] UpdateBookDto dto)
    {
        var result = await _bookService.UpdateBookAsync(id, dto);
        return Ok(result);
    }

    /// <summary>Delete a book (fails if the book has active loans)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteBook(int id)
    {
        await _bookService.DeleteBookAsync(id);
        return NoContent();
    }

    /// <summary>Get loan history for a specific book</summary>
    [HttpGet("{id}/loans")]
    [ProducesResponseType(typeof(PagedResult<LoanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetBookLoans(int id,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _bookService.GetBookLoansAsync(id, new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get active reservations queue for a specific book</summary>
    [HttpGet("{id}/reservations")]
    [ProducesResponseType(typeof(PagedResult<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<ReservationDto>>> GetBookReservations(int id,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _bookService.GetBookReservationsAsync(id, new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }
}
