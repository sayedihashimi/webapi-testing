using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _service;

    public LoansController(ILoanService service) => _service = service;

    /// <summary>List loans with filters (status, overdue, date range) and pagination</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetAll(
        [FromQuery] LoanStatus? status,
        [FromQuery] bool? overdue,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;
        return Ok(await _service.GetAllAsync(status, overdue, fromDate, toDate, page, pageSize));
    }

    /// <summary>Get loan details</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<LoanDto>> GetById(int id)
    {
        var loan = await _service.GetByIdAsync(id);
        return loan is null ? NotFound() : Ok(loan);
    }

    /// <summary>Checkout a book (creates a loan, enforces all business rules)</summary>
    [HttpPost]
    public async Task<ActionResult<LoanDto>> Checkout(LoanCreateDto dto)
    {
        var loan = await _service.CheckoutAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }

    /// <summary>Return a book</summary>
    [HttpPost("{id}/return")]
    public async Task<ActionResult<LoanDto>> Return(int id)
    {
        return Ok(await _service.ReturnAsync(id));
    }

    /// <summary>Renew a loan</summary>
    [HttpPost("{id}/renew")]
    public async Task<ActionResult<LoanDto>> Renew(int id)
    {
        return Ok(await _service.RenewAsync(id));
    }

    /// <summary>Get all overdue loans</summary>
    [HttpGet("overdue")]
    public async Task<ActionResult<List<LoanDto>>> GetOverdue()
    {
        return Ok(await _service.GetOverdueAsync());
    }
}
