using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingsController(IBookingService service) => _service = service;

    /// <summary>Book a class (enforces all booking rules)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Get booking details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookingDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    /// <summary>Cancel a booking (enforces cancellation policy, promotes from waitlist)</summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(BookingDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelBookingDto dto)
        => Ok(await _service.CancelAsync(id, dto));

    /// <summary>Check in for a class (enforces check-in window)</summary>
    [HttpPost("{id}/check-in")]
    [ProducesResponseType(typeof(BookingDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CheckIn(int id) => Ok(await _service.CheckInAsync(id));

    /// <summary>Mark booking as no-show</summary>
    [HttpPost("{id}/no-show")]
    [ProducesResponseType(typeof(BookingDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> NoShow(int id) => Ok(await _service.NoShowAsync(id));
}
