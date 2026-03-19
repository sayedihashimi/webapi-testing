using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;

    public ReservationsController(IReservationService service) => _service = service;

    /// <summary>List reservations with filter by status and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReservationDto>), 200)]
    public async Task<IActionResult> GetReservations([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetReservationsAsync(status, page, pageSize));

    /// <summary>Get reservation details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReservationDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetReservation(int id)
        => Ok(await _service.GetReservationByIdAsync(id));

    /// <summary>Create a reservation enforcing all reservation rules</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateReservation([FromBody] ReservationCreateDto dto)
    {
        var reservation = await _service.CreateReservationAsync(dto);
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }

    /// <summary>Cancel a reservation</summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ReservationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelReservation(int id)
        => Ok(await _service.CancelReservationAsync(id));

    /// <summary>Fulfill a Ready reservation (creates a loan for the patron)</summary>
    [HttpPost("{id}/fulfill")]
    [ProducesResponseType(typeof(LoanDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> FulfillReservation(int id)
        => Ok(await _service.FulfillReservationAsync(id));
}
