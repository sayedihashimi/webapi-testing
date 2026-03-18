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

    /// <summary>List loans with filter by status, overdue flag, and date range</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<LoanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] bool? overdue,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _loanService.GetAllAsync(status, overdue, fromDate, toDate, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get loan details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var loan = await _loanService.GetByIdAsync(id);
        if (loan == null) return NotFound(new ProblemDetails { Title = "Loan not found", Status = 404 });
        return Ok(loan);
    }

    /// <summary>Check out a book — create a loan enforcing all checkout rules</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Checkout([FromBody] CreateLoanDto dto)
    {
        var (loan, error) = await _loanService.CheckoutAsync(dto);
        if (loan == null)
            return BadRequest(new ProblemDetails { Title = "Checkout denied", Detail = error, Status = 400 });
        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }

    /// <summary>Return a book — enforce all return processing rules</summary>
    [HttpPost("{id}/return")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Return(int id)
    {
        var (loan, error) = await _loanService.ReturnAsync(id);
        if (loan == null)
        {
            if (error == "Loan not found.")
                return NotFound(new ProblemDetails { Title = "Loan not found", Status = 404 });
            return BadRequest(new ProblemDetails { Title = "Return failed", Detail = error, Status = 400 });
        }
        return Ok(loan);
    }

    /// <summary>Renew a loan — enforce all renewal rules</summary>
    [HttpPost("{id}/renew")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Renew(int id)
    {
        var (loan, error) = await _loanService.RenewAsync(id);
        if (loan == null)
        {
            if (error == "Loan not found.")
                return NotFound(new ProblemDetails { Title = "Loan not found", Status = 404 });
            return BadRequest(new ProblemDetails { Title = "Renewal denied", Detail = error, Status = 400 });
        }
        return Ok(loan);
    }

    /// <summary>Get all currently overdue loans (also flags active loans as overdue)</summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(PaginatedResponse<LoanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdue([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _loanService.GetOverdueAsync(page, pageSize);
        return Ok(result);
    }
}
