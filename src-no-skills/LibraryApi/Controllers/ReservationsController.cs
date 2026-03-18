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

    /// <summary>List reservations with filter by status and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _reservationService.GetAllAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get reservation details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var reservation = await _reservationService.GetByIdAsync(id);
        if (reservation == null) return NotFound(new ProblemDetails { Title = "Reservation not found", Status = 404 });
        return Ok(reservation);
    }

    /// <summary>Create a reservation enforcing all reservation rules</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
    {
        var (reservation, error) = await _reservationService.CreateAsync(dto);
        if (reservation == null)
            return BadRequest(new ProblemDetails { Title = "Reservation denied", Detail = error, Status = 400 });
        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    /// <summary>Cancel a reservation</summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id)
    {
        var (reservation, error) = await _reservationService.CancelAsync(id);
        if (reservation == null)
        {
            if (error == "Reservation not found.")
                return NotFound(new ProblemDetails { Title = "Reservation not found", Status = 404 });
            return BadRequest(new ProblemDetails { Title = "Cancel failed", Detail = error, Status = 400 });
        }
        return Ok(reservation);
    }

    /// <summary>Fulfill a "Ready" reservation (creates a loan for the patron)</summary>
    [HttpPost("{id}/fulfill")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fulfill(int id)
    {
        var (loan, error) = await _reservationService.FulfillAsync(id);
        if (loan == null)
        {
            if (error == "Reservation not found.")
                return NotFound(new ProblemDetails { Title = "Reservation not found", Status = 404 });
            return BadRequest(new ProblemDetails { Title = "Fulfillment failed", Detail = error, Status = 400 });
        }
        return Ok(loan);
    }
}
