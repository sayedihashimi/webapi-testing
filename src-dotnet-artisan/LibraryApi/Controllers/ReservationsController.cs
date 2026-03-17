using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;

    public ReservationsController(IReservationService service) => _service = service;

    /// <summary>List reservations with optional status filter and pagination</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ReservationDto>>> GetAll(
        [FromQuery] ReservationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;
        return Ok(await _service.GetAllAsync(status, page, pageSize));
    }

    /// <summary>Get reservation details</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ReservationDto>> GetById(int id)
    {
        var reservation = await _service.GetByIdAsync(id);
        return reservation is null ? NotFound() : Ok(reservation);
    }

    /// <summary>Create a reservation</summary>
    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create(ReservationCreateDto dto)
    {
        var reservation = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    /// <summary>Cancel a reservation</summary>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ReservationDto>> Cancel(int id)
    {
        return Ok(await _service.CancelAsync(id));
    }

    /// <summary>Fulfill a Ready reservation (creates a loan)</summary>
    [HttpPost("{id}/fulfill")]
    public async Task<ActionResult<LoanDto>> Fulfill(int id)
    {
        return Ok(await _service.FulfillAsync(id));
    }
}
