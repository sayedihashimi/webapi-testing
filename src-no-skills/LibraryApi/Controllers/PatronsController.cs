using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatronsController : ControllerBase
{
    private readonly IPatronService _patronService;

    public PatronsController(IPatronService patronService)
    {
        _patronService = patronService;
    }

    /// <summary>List patrons with search and filter by membership type</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<PatronDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] MembershipType? membershipType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _patronService.GetAllAsync(search, membershipType, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get patron details with summary (active loans, unpaid fines)</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PatronDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var patron = await _patronService.GetByIdAsync(id);
        if (patron == null) return NotFound(new ProblemDetails { Title = "Patron not found", Status = 404 });
        return Ok(patron);
    }

    /// <summary>Create a new patron</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PatronDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePatronDto dto)
    {
        var patron = await _patronService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = patron.Id }, patron);
    }

    /// <summary>Update an existing patron</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PatronDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePatronDto dto)
    {
        var patron = await _patronService.UpdateAsync(id, dto);
        if (patron == null) return NotFound(new ProblemDetails { Title = "Patron not found", Status = 404 });
        return Ok(patron);
    }

    /// <summary>Deactivate patron (set IsActive = false; fails if patron has active loans)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(int id)
    {
        var (success, error) = await _patronService.DeactivateAsync(id);
        if (!success)
        {
            if (error == "Patron not found")
                return NotFound(new ProblemDetails { Title = error, Status = 404 });
            return Conflict(new ProblemDetails { Title = "Deactivation failed", Detail = error, Status = 409 });
        }
        return NoContent();
    }

    /// <summary>Get patron's loans (filter by status)</summary>
    [HttpGet("{id}/loans")]
    [ProducesResponseType(typeof(PaginatedResponse<LoanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatronLoans(int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _patronService.GetPatronLoansAsync(id, status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get patron's reservations</summary>
    [HttpGet("{id}/reservations")]
    [ProducesResponseType(typeof(PaginatedResponse<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatronReservations(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _patronService.GetPatronReservationsAsync(id, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get patron's fines (filter by status)</summary>
    [HttpGet("{id}/fines")]
    [ProducesResponseType(typeof(PaginatedResponse<FineDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatronFines(int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _patronService.GetPatronFinesAsync(id, status, page, pageSize);
        return Ok(result);
    }
}
