using LibraryApi.DTOs;
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

    /// <summary>List patrons with search by name/email, filter by membership type, pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PatronDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PatronDto>>> GetPatrons(
        [FromQuery] string? search,
        [FromQuery] string? membershipType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _patronService.GetPatronsAsync(search, membershipType,
            new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get patron details with active loans count and unpaid fines balance</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PatronDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatronDetailDto>> GetPatron(int id)
    {
        var result = await _patronService.GetPatronByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Create a new patron</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PatronDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PatronDto>> CreatePatron([FromBody] CreatePatronDto dto)
    {
        var result = await _patronService.CreatePatronAsync(dto);
        return CreatedAtAction(nameof(GetPatron), new { id = result.Id }, result);
    }

    /// <summary>Update an existing patron</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PatronDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PatronDto>> UpdatePatron(int id, [FromBody] UpdatePatronDto dto)
    {
        var result = await _patronService.UpdatePatronAsync(id, dto);
        return Ok(result);
    }

    /// <summary>Deactivate a patron (fails if patron has active loans)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeactivatePatron(int id)
    {
        await _patronService.DeactivatePatronAsync(id);
        return NoContent();
    }

    /// <summary>Get patron's loans with optional status filter</summary>
    [HttpGet("{id}/loans")]
    [ProducesResponseType(typeof(PagedResult<LoanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetPatronLoans(int id,
        [FromQuery] string? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _patronService.GetPatronLoansAsync(id, status,
            new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get patron's reservations</summary>
    [HttpGet("{id}/reservations")]
    [ProducesResponseType(typeof(PagedResult<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<ReservationDto>>> GetPatronReservations(int id,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _patronService.GetPatronReservationsAsync(id,
            new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get patron's fines with optional status filter</summary>
    [HttpGet("{id}/fines")]
    [ProducesResponseType(typeof(PagedResult<FineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<FineDto>>> GetPatronFines(int id,
        [FromQuery] string? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _patronService.GetPatronFinesAsync(id, status,
            new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }
}
