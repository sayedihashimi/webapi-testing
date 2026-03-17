using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController(IBookingService service) : ControllerBase
{
    /// <summary>Get booking details</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingDto>> GetById(int id)
    {
        var booking = await service.GetByIdAsync(id);
        return booking is null ? NotFound() : Ok(booking);
    }

    /// <summary>Book a class (enforces all business rules)</summary>
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingDto dto)
    {
        var (result, error) = await service.CreateAsync(dto);
        if (result is null) return BadRequest(new { error });
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Cancel a booking (enforces cancellation policy, promotes waitlist)</summary>
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancelBookingDto dto)
    {
        var (success, error) = await service.CancelAsync(id, dto);
        if (!success) return error == "Booking not found." ? NotFound() : BadRequest(new { error });
        return Ok(new { message = "Booking cancelled." });
    }

    /// <summary>Check in to a class (15 min window)</summary>
    [HttpPost("{id:int}/check-in")]
    public async Task<IActionResult> CheckIn(int id)
    {
        var (success, error) = await service.CheckInAsync(id);
        if (!success) return error == "Booking not found." ? NotFound() : BadRequest(new { error });
        return Ok(new { message = "Checked in successfully." });
    }

    /// <summary>Mark booking as no-show</summary>
    [HttpPost("{id:int}/no-show")]
    public async Task<IActionResult> NoShow(int id)
    {
        var (success, error) = await service.MarkNoShowAsync(id);
        if (!success) return error == "Booking not found." ? NotFound() : BadRequest(new { error });
        return Ok(new { message = "Marked as no-show." });
    }
}
