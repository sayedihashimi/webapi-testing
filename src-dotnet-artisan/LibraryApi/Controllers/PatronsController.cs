using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatronsController : ControllerBase
{
    private readonly IPatronService _service;

    public PatronsController(IPatronService service) => _service = service;

    /// <summary>List patrons with search, membership filter, pagination</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<PatronDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] MembershipType? membershipType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;
        return Ok(await _service.GetAllAsync(search, membershipType, page, pageSize));
    }

    /// <summary>Get patron details with active loans count and unpaid fines</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PatronDetailDto>> GetById(int id)
    {
        var patron = await _service.GetByIdAsync(id);
        return patron is null ? NotFound() : Ok(patron);
    }

    /// <summary>Create a new patron</summary>
    [HttpPost]
    public async Task<ActionResult<PatronDto>> Create(PatronCreateDto dto)
    {
        var patron = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = patron.Id }, patron);
    }

    /// <summary>Update a patron</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<PatronDto>> Update(int id, PatronUpdateDto dto)
    {
        var patron = await _service.UpdateAsync(id, dto);
        return patron is null ? NotFound() : Ok(patron);
    }

    /// <summary>Deactivate a patron (fails if active loans)</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get patron's loans with optional status filter</summary>
    [HttpGet("{id}/loans")]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetLoans(
        int id,
        [FromQuery] LoanStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;
        return Ok(await _service.GetLoansAsync(id, status, page, pageSize));
    }

    /// <summary>Get patron's active reservations</summary>
    [HttpGet("{id}/reservations")]
    public async Task<ActionResult<List<ReservationDto>>> GetReservations(int id)
    {
        return Ok(await _service.GetReservationsAsync(id));
    }

    /// <summary>Get patron's fines with optional status filter</summary>
    [HttpGet("{id}/fines")]
    public async Task<ActionResult<List<FineDto>>> GetFines(int id, [FromQuery] FineStatus? status)
    {
        return Ok(await _service.GetFinesAsync(id, status));
    }
}
