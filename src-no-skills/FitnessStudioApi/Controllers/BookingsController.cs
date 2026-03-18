using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingsController(IBookingService service) => _service = service;

    /// <summary>Book a class (enforces all business rules)</summary>
    [HttpPost]
    [ProducesResponseType<BookingDto>(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBookingDto dto,
        [FromServices] IValidator<CreateBookingDto> validator)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var booking = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    /// <summary>Get booking details</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<BookingDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _service.GetByIdAsync(id);
        return booking is null ? NotFound() : Ok(booking);
    }

    /// <summary>Cancel a booking (promotes waitlisted member if applicable)</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType<BookingDto>(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelBookingDto dto)
        => Ok(await _service.CancelAsync(id, dto));

    /// <summary>Check in for a class (15 min window around class start)</summary>
    [HttpPost("{id:int}/check-in")]
    [ProducesResponseType<BookingDto>(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CheckIn(int id)
        => Ok(await _service.CheckInAsync(id));

    /// <summary>Mark booking as no-show (available 15 min after class start)</summary>
    [HttpPost("{id:int}/no-show")]
    [ProducesResponseType<BookingDto>(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> NoShow(int id)
        => Ok(await _service.MarkNoShowAsync(id));
}
