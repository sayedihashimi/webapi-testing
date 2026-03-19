using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _service;

    public LoansController(ILoanService service) => _service = service;

    /// <summary>List loans with filter by status, overdue flag, date range, and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LoanDto>), 200)]
    public async Task<IActionResult> GetLoans(
        [FromQuery] string? status,
        [FromQuery] bool? overdue,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        => Ok(await _service.GetLoansAsync(status, overdue, fromDate, toDate, page, pageSize));

    /// <summary>Get loan details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LoanDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetLoan(int id)
        => Ok(await _service.GetLoanByIdAsync(id));

    /// <summary>Check out a book — creates a loan enforcing all checkout rules</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LoanDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CheckoutBook([FromBody] LoanCreateDto dto)
    {
        var loan = await _service.CheckoutBookAsync(dto);
        return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loan);
    }

    /// <summary>Return a book — enforces all return processing rules</summary>
    [HttpPost("{id}/return")]
    [ProducesResponseType(typeof(LoanDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ReturnBook(int id)
        => Ok(await _service.ReturnBookAsync(id));

    /// <summary>Renew a loan — enforces all renewal rules</summary>
    [HttpPost("{id}/renew")]
    [ProducesResponseType(typeof(LoanDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RenewLoan(int id)
        => Ok(await _service.RenewLoanAsync(id));

    /// <summary>Get all currently overdue loans (also flags overdue loans)</summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(PagedResult<LoanDto>), 200)]
    public async Task<IActionResult> GetOverdueLoans([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetOverdueLoansAsync(page, pageSize));
}
