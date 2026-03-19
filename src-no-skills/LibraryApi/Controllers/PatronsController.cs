using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Fine;
using LibraryApi.DTOs.Loan;
using LibraryApi.DTOs.Patron;
using LibraryApi.DTOs.Reservation;
using LibraryApi.Models.Enums;
using LibraryApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatronsController : ControllerBase
{
    private readonly IPatronService _service;

    public PatronsController(IPatronService service) => _service = service;

    /// <summary>List patrons with search, filter, pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PatronListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search, [FromQuery] MembershipType? membershipType,
        [FromQuery] bool? isActive, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetAllAsync(search, membershipType, isActive, pagination));

    /// <summary>Get patron details with summary.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PatronDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Create a new patron.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PatronDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePatronDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a patron.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PatronDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePatronDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Deactivate a patron (fails if active loans).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get patron loans (filter by status).</summary>
    [HttpGet("{id}/loans")]
    [ProducesResponseType(typeof(PagedResult<LoanListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoans(int id, [FromQuery] LoanStatus? status, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetPatronLoansAsync(id, status, pagination));

    /// <summary>Get patron reservations.</summary>
    [HttpGet("{id}/reservations")]
    [ProducesResponseType(typeof(List<ReservationListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservations(int id)
        => Ok(await _service.GetPatronReservationsAsync(id));

    /// <summary>Get patron fines (filter by status).</summary>
    [HttpGet("{id}/fines")]
    [ProducesResponseType(typeof(PagedResult<FineListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFines(int id, [FromQuery] FineStatus? status, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetPatronFinesAsync(id, status, pagination));
}
