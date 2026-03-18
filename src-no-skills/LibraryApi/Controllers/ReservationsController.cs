using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    /// <summary>List reservations with optional status filter and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReservationDto>>> GetReservations(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _reservationService.GetReservationsAsync(status,
            new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get reservation details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> GetReservation(int id)
    {
        var result = await _reservationService.GetReservationByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Create a reservation enforcing all reservation rules</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> CreateReservation([FromBody] CreateReservationDto dto)
    {
        var result = await _reservationService.CreateReservationAsync(dto);
        return CreatedAtAction(nameof(GetReservation), new { id = result.Id }, result);
    }

    /// <summary>Cancel a reservation</summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> CancelReservation(int id)
    {
        var result = await _reservationService.CancelReservationAsync(id);
        return Ok(result);
    }

    /// <summary>Fulfill a Ready reservation (creates a loan for the patron)</summary>
    [HttpPost("{id}/fulfill")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanDto>> FulfillReservation(int id)
    {
        var result = await _reservationService.FulfillReservationAsync(id);
        return Ok(result);
    }
}
