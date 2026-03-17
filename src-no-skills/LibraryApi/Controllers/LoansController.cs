using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly LoanService _service;

    public LoansController(LoanService service) => _service = service;

    /// <summary>Get all loans with filters for status, overdue, date range, and pagination.</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<LoanDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] bool? overdue,
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(status, overdue, fromDate, toDate, page, pageSize));

    /// <summary>Get loan by ID.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<LoanDto>> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Checkout a book (create a loan). Enforces all borrowing rules.</summary>
    [HttpPost]
    public async Task<ActionResult<LoanDto>> Checkout(CreateLoanDto dto)
    {
        var loan = await _service.CheckoutAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }

    /// <summary>Return a book.</summary>
    [HttpPost("{id}/return")]
    public async Task<ActionResult<LoanDto>> Return(int id)
        => Ok(await _service.ReturnAsync(id));

    /// <summary>Renew a loan (max 2 renewals).</summary>
    [HttpPost("{id}/renew")]
    public async Task<ActionResult<LoanDto>> Renew(int id)
        => Ok(await _service.RenewAsync(id));

    /// <summary>Get all overdue loans. Also flags active-but-overdue loans.</summary>
    [HttpGet("overdue")]
    public async Task<ActionResult<List<LoanDto>>> GetOverdue()
        => Ok(await _service.GetOverdueAsync());
}
