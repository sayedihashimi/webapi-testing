using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly BookingService _service;

    public BookingsController(BookingService service)
    {
        _service = service;
    }

    /// <summary>Create a new booking (enforces all business rules)</summary>
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingDto dto)
    {
        var booking = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    /// <summary>Get a booking by ID</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BookingDto>> GetById(int id)
    {
        var booking = await _service.GetByIdAsync(id);
        return booking == null ? NotFound() : Ok(booking);
    }

    /// <summary>Cancel a booking (enforces cancellation policy, promotes waitlist)</summary>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<BookingDto>> Cancel(int id, CancelBookingDto dto)
        => Ok(await _service.CancelAsync(id, dto));

    /// <summary>Check in to a class (15 min window around start time)</summary>
    [HttpPost("{id}/check-in")]
    public async Task<ActionResult<BookingDto>> CheckIn(int id)
        => Ok(await _service.CheckInAsync(id));

    /// <summary>Mark a booking as no-show (after 15 min past start)</summary>
    [HttpPost("{id}/no-show")]
    public async Task<ActionResult<BookingDto>> NoShow(int id)
        => Ok(await _service.NoShowAsync(id));
}
