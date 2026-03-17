using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationService _service;

    public ReservationsController(ReservationService service) => _service = service;

    /// <summary>Get all reservations with optional status filter and pagination.</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ReservationDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(status, page, pageSize));

    /// <summary>Get reservation by ID.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ReservationDto>> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Create a new reservation.</summary>
    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create(CreateReservationDto dto)
    {
        var reservation = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    /// <summary>Cancel a reservation.</summary>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ReservationDto>> Cancel(int id)
        => Ok(await _service.CancelAsync(id));

    /// <summary>Fulfill a reservation (creates a loan).</summary>
    [HttpPost("{id}/fulfill")]
    public async Task<ActionResult<LoanDto>> Fulfill(int id)
    {
        var loan = await _service.FulfillAsync(id);
        return CreatedAtAction("GetById", "Loans", new { id = loan.Id }, loan);
    }
}
