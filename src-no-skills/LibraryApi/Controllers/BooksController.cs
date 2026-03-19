using LibraryApi.DTOs.Book;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loan;
using LibraryApi.DTOs.Reservation;
using LibraryApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    private readonly IBookService _service;

    public BooksController(IBookService service) => _service = service;

    /// <summary>List books with search, filter, pagination, sorting.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BookListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search, [FromQuery] int? categoryId, [FromQuery] int? authorId,
        [FromQuery] string? sortBy, [FromQuery] string? sortOrder,
        [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetAllAsync(search, categoryId, authorId, sortBy, sortOrder, pagination));

    /// <summary>Get book details with authors, categories, availability.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Create a book with author/category IDs.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a book.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BookDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Delete a book (fails if active loans).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get loan history for a book.</summary>
    [HttpGet("{id}/loans")]
    [ProducesResponseType(typeof(PagedResult<LoanListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoans(int id, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetBookLoansAsync(id, pagination));

    /// <summary>Get active reservations for a book.</summary>
    [HttpGet("{id}/reservations")]
    [ProducesResponseType(typeof(List<ReservationListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservations(int id)
        => Ok(await _service.GetBookReservationsAsync(id));
}
