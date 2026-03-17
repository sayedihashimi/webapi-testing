using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatronsController : ControllerBase
{
    private readonly PatronService _service;

    public PatronsController(PatronService service) => _service = service;

    /// <summary>Get all patrons with search, membership type filter, and pagination.</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<PatronDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] MembershipType? membershipType,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(search, membershipType, page, pageSize));

    /// <summary>Get patron by ID with active loans count and fines balance.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PatronDetailDto>> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Create a new patron.</summary>
    [HttpPost]
    public async Task<ActionResult<PatronDto>> Create(CreatePatronDto dto)
    {
        var patron = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = patron.Id }, patron);
    }

    /// <summary>Update an existing patron.</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<PatronDto>> Update(int id, UpdatePatronDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Deactivate a patron (fails if active loans).</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get patron's loans with optional status filter.</summary>
    [HttpGet("{id}/loans")]
    public async Task<ActionResult<PaginatedResponse<LoanDto>>> GetLoans(
        int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetPatronLoansAsync(id, status, page, pageSize));

    /// <summary>Get patron's reservations.</summary>
    [HttpGet("{id}/reservations")]
    public async Task<ActionResult<PaginatedResponse<ReservationDto>>> GetReservations(
        int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetPatronReservationsAsync(id, page, pageSize));

    /// <summary>Get patron's fines with optional status filter.</summary>
    [HttpGet("{id}/fines")]
    public async Task<ActionResult<PaginatedResponse<FineDto>>> GetFines(
        int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetPatronFinesAsync(id, status, page, pageSize));
}
