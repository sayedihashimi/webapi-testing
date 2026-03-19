using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/members")]
[Produces("application/json")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _service;

    public MembersController(IMemberService service)
    {
        _service = service;
    }

    /// <summary>List members with search, filtering, and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MemberResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? isActive,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetAllAsync(search, isActive, new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get member details including active membership</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MemberResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var member = await _service.GetByIdAsync(id);
        return member is null ? NotFound() : Ok(member);
    }

    /// <summary>Register a new member</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MemberResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] MemberCreateDto dto)
    {
        var member = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    /// <summary>Update member profile</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MemberResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int id, [FromBody] MemberUpdateDto dto)
    {
        var member = await _service.UpdateAsync(id, dto);
        return member is null ? NotFound() : Ok(member);
    }

    /// <summary>Deactivate a member (fails if they have future bookings)</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeactivateAsync(id);
        return result ? NoContent() : NotFound();
    }

    /// <summary>Get member's bookings with filtering and pagination</summary>
    [HttpGet("{id:int}/bookings")]
    [ProducesResponseType(typeof(PagedResult<BookingResponseDto>), 200)]
    public async Task<IActionResult> GetBookings(int id, [FromQuery] string? status,
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetBookingsAsync(id, status, fromDate, toDate,
            new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get member's upcoming confirmed bookings</summary>
    [HttpGet("{id:int}/bookings/upcoming")]
    [ProducesResponseType(typeof(List<BookingResponseDto>), 200)]
    public async Task<IActionResult> GetUpcomingBookings(int id)
    {
        var result = await _service.GetUpcomingBookingsAsync(id);
        return Ok(result);
    }

    /// <summary>Get membership history for a member</summary>
    [HttpGet("{id:int}/memberships")]
    [ProducesResponseType(typeof(List<MembershipResponseDto>), 200)]
    public async Task<IActionResult> GetMemberships(int id)
    {
        var result = await _service.GetMembershipsAsync(id);
        return Ok(result);
    }
}
