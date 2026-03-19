using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Reservation;
using LibraryApi.Models.Enums;
using LibraryApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;

    public ReservationsController(IReservationService service) => _service = service;

    /// <summary>List reservations with filter, pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReservationListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] ReservationStatus? status, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetAllAsync(status, pagination));

    /// <summary>Get reservation details.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReservationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Create a reservation (enforce rules).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Cancel a reservation.</summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id)
    {
        await _service.CancelAsync(id);
        return NoContent();
    }

    /// <summary>Fulfill a "Ready" reservation.</summary>
    [HttpPost("{id}/fulfill")]
    [ProducesResponseType(typeof(ReservationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Fulfill(int id)
        => Ok(await _service.FulfillAsync(id));
}
