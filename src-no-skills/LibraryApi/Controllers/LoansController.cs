using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    /// <summary>List loans with filters for status, overdue flag, date range, and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LoanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetLoans(
        [FromQuery] string? status,
        [FromQuery] bool? overdue,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _loanService.GetLoansAsync(status, overdue, fromDate, toDate,
            new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get loan details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanDto>> GetLoan(int id)
    {
        var result = await _loanService.GetLoanByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Check out a book — creates a loan enforcing all checkout rules</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanDto>> CheckoutBook([FromBody] CreateLoanDto dto)
    {
        var result = await _loanService.CheckoutBookAsync(dto);
        return CreatedAtAction(nameof(GetLoan), new { id = result.Id }, result);
    }

    /// <summary>Return a book — enforces all return processing rules</summary>
    [HttpPost("{id}/return")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanDto>> ReturnBook(int id)
    {
        var result = await _loanService.ReturnBookAsync(id);
        return Ok(result);
    }

    /// <summary>Renew a loan — enforces all renewal rules</summary>
    [HttpPost("{id}/renew")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanDto>> RenewLoan(int id)
    {
        var result = await _loanService.RenewLoanAsync(id);
        return Ok(result);
    }

    /// <summary>Get all currently overdue loans (also flags active loans past due date)</summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(PagedResult<LoanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetOverdueLoans(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _loanService.GetOverdueLoansAsync(new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }
}
