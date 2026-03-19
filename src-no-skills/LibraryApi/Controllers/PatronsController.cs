using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatronsController : ControllerBase
{
    private readonly IPatronService _service;

    public PatronsController(IPatronService service) => _service = service;

    /// <summary>List patrons with search by name/email, filter by membership type, pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PatronDto>), 200)]
    public async Task<IActionResult> GetPatrons(
        [FromQuery] string? search,
        [FromQuery] string? membershipType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        => Ok(await _service.GetPatronsAsync(search, membershipType, page, pageSize));

    /// <summary>Get patron details with active loans count and unpaid fines balance</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PatronDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPatron(int id)
        => Ok(await _service.GetPatronByIdAsync(id));

    /// <summary>Create a new patron</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PatronDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreatePatron([FromBody] PatronCreateDto dto)
    {
        var patron = await _service.CreatePatronAsync(dto);
        return CreatedAtAction(nameof(GetPatron), new { id = patron.Id }, patron);
    }

    /// <summary>Update an existing patron</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PatronDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> UpdatePatron(int id, [FromBody] PatronUpdateDto dto)
        => Ok(await _service.UpdatePatronAsync(id, dto));

    /// <summary>Deactivate patron (fails if patron has active loans)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> DeactivatePatron(int id)
    {
        await _service.DeactivatePatronAsync(id);
        return NoContent();
    }

    /// <summary>Get patron's loans with optional status filter</summary>
    [HttpGet("{id}/loans")]
    [ProducesResponseType(typeof(PagedResult<LoanDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPatronLoans(int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetPatronLoansAsync(id, status, page, pageSize));

    /// <summary>Get patron's reservations</summary>
    [HttpGet("{id}/reservations")]
    [ProducesResponseType(typeof(PagedResult<ReservationDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPatronReservations(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetPatronReservationsAsync(id, page, pageSize));

    /// <summary>Get patron's fines with optional status filter</summary>
    [HttpGet("{id}/fines")]
    [ProducesResponseType(typeof(PagedResult<FineDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPatronFines(int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetPatronFinesAsync(id, status, page, pageSize));
}
