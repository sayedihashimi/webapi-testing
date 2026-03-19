using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loan;
using LibraryApi.Models.Enums;
using LibraryApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _service;

    public LoansController(ILoanService service) => _service = service;

    /// <summary>List loans with filters, pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LoanListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] LoanStatus? status, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetAllAsync(status, pagination));

    /// <summary>Get loan details.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LoanDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Checkout a book (enforce all borrowing rules).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LoanDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Checkout([FromBody] CreateLoanDto dto)
    {
        var result = await _service.CheckoutAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Return a book (process overdue fines, promotions).</summary>
    [HttpPost("{id}/return")]
    [ProducesResponseType(typeof(ReturnLoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Return(int id)
        => Ok(await _service.ReturnAsync(id));

    /// <summary>Renew a loan (enforce renewal rules).</summary>
    [HttpPost("{id}/renew")]
    [ProducesResponseType(typeof(RenewLoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Renew(int id)
        => Ok(await _service.RenewAsync(id));

    /// <summary>Get all overdue loans.</summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(List<LoanListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdue()
        => Ok(await _service.GetOverdueLoansAsync());
}
