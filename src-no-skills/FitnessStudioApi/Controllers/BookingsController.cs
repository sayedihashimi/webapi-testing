using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingsController(IBookingService service)
    {
        _service = service;
    }

    /// <summary>Book a class (enforces all booking rules)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] BookingCreateDto dto)
    {
        var booking = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    /// <summary>Get booking details</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookingResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _service.GetByIdAsync(id);
        return booking is null ? NotFound() : Ok(booking);
    }

    /// <summary>Cancel a booking (enforces cancellation policy, promotes from waitlist)</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(BookingResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelBookingDto? dto)
    {
        var result = await _service.CancelAsync(id, dto);
        return Ok(result);
    }

    /// <summary>Check in for a class (15 min before to 15 min after start)</summary>
    [HttpPost("{id:int}/check-in")]
    [ProducesResponseType(typeof(BookingResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CheckIn(int id)
    {
        var result = await _service.CheckInAsync(id);
        return Ok(result);
    }

    /// <summary>Mark booking as no-show</summary>
    [HttpPost("{id:int}/no-show")]
    [ProducesResponseType(typeof(BookingResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> MarkNoShow(int id)
    {
        var result = await _service.MarkNoShowAsync(id);
        return Ok(result);
    }
}
