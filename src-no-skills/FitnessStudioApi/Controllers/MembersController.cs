using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/members")]
[Produces("application/json")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _service;

    public MembersController(IMemberService service) => _service = service;

    /// <summary>List members with search, filter, and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MemberListDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetAllAsync(search, isActive, pagination));

    /// <summary>Get member details including active membership and booking stats</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MemberDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    /// <summary>Register a new member</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MemberDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateMemberDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update member profile</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MemberDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Deactivate a member (fails if they have future bookings)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get member's bookings with filters and pagination</summary>
    [HttpGet("{id}/bookings")]
    [ProducesResponseType(typeof(PagedResult<BookingDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBookings(int id, [FromQuery] string? status, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetBookingsAsync(id, status, fromDate, toDate, pagination));

    /// <summary>Get member's upcoming confirmed bookings</summary>
    [HttpGet("{id}/bookings/upcoming")]
    [ProducesResponseType(typeof(List<BookingDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUpcomingBookings(int id)
        => Ok(await _service.GetUpcomingBookingsAsync(id));

    /// <summary>Get membership history for a member</summary>
    [HttpGet("{id}/memberships")]
    [ProducesResponseType(typeof(List<MembershipDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMemberships(int id)
        => Ok(await _service.GetMembershipsAsync(id));
}
