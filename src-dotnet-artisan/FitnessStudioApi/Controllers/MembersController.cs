using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/members")]
public class MembersController(IMemberService service) : ControllerBase
{
    /// <summary>List members with search, filter, and pagination</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<MemberDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] bool? isActive,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await service.GetAllAsync(search, isActive, page, pageSize));

    /// <summary>Get member details with active membership and booking stats</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MemberDetailDto>> GetById(int id)
    {
        var member = await service.GetByIdAsync(id);
        return member is null ? NotFound() : Ok(member);
    }

    /// <summary>Register a new member</summary>
    [HttpPost]
    public async Task<ActionResult<MemberDto>> Create(CreateMemberDto dto)
    {
        var member = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    /// <summary>Update member profile</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MemberDto>> Update(int id, UpdateMemberDto dto)
    {
        var member = await service.UpdateAsync(id, dto);
        return member is null ? NotFound() : Ok(member);
    }

    /// <summary>Deactivate member (fails if future bookings exist)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await service.DeactivateAsync(id);
        if (!success) return error == "Member not found." ? NotFound() : BadRequest(new { error });
        return NoContent();
    }

    /// <summary>Get member bookings with filters and pagination</summary>
    [HttpGet("{id:int}/bookings")]
    public async Task<ActionResult<PaginatedResult<BookingDto>>> GetBookings(
        int id, [FromQuery] string? status, [FromQuery] DateTime? from,
        [FromQuery] DateTime? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await service.GetBookingsAsync(id, status, from, to, page, pageSize));

    /// <summary>Get upcoming confirmed bookings</summary>
    [HttpGet("{id:int}/bookings/upcoming")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> GetUpcomingBookings(int id)
        => Ok(await service.GetUpcomingBookingsAsync(id));

    /// <summary>Get membership history</summary>
    [HttpGet("{id:int}/memberships")]
    public async Task<ActionResult<IReadOnlyList<MembershipDto>>> GetMemberships(int id)
        => Ok(await service.GetMembershipsAsync(id));
}
