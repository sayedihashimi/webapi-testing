using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/members")]
[Produces("application/json")]
public class MembersController : ControllerBase
{
    private readonly MemberService _service;

    public MembersController(MemberService service)
    {
        _service = service;
    }

    /// <summary>Get all members with search, filtering, and pagination</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<MemberDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] bool? isActive,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(search, isActive, page, pageSize));

    /// <summary>Get a member by ID with membership info and booking stats</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDetailDto>> GetById(int id)
    {
        var member = await _service.GetByIdAsync(id);
        return member == null ? NotFound() : Ok(member);
    }

    /// <summary>Create a new member</summary>
    [HttpPost]
    public async Task<ActionResult<MemberDto>> Create(CreateMemberDto dto)
    {
        var member = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    /// <summary>Update a member</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<MemberDto>> Update(int id, UpdateMemberDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Deactivate a member (fails if future bookings exist)</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get a member's bookings with filtering and pagination</summary>
    [HttpGet("{id}/bookings")]
    public async Task<ActionResult<PaginatedResponse<BookingDto>>> GetBookings(
        int id, [FromQuery] string? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetBookingsAsync(id, status, from, to, page, pageSize));

    /// <summary>Get a member's upcoming bookings</summary>
    [HttpGet("{id}/bookings/upcoming")]
    public async Task<ActionResult<List<BookingDto>>> GetUpcomingBookings(int id)
        => Ok(await _service.GetUpcomingBookingsAsync(id));

    /// <summary>Get a member's memberships</summary>
    [HttpGet("{id}/memberships")]
    public async Task<ActionResult<List<MembershipDto>>> GetMemberships(int id)
        => Ok(await _service.GetMembershipsAsync(id));
}
