using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType<PagedResult<MemberDto>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search, [FromQuery] bool? isActive,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(search, isActive, page, pageSize));

    /// <summary>Get member details including active membership and booking stats</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<MemberDetailDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var member = await _service.GetByIdAsync(id);
        return member is null ? NotFound() : Ok(member);
    }

    /// <summary>Register a new member</summary>
    [HttpPost]
    [ProducesResponseType<MemberDto>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMemberDto dto,
        [FromServices] IValidator<CreateMemberDto> validator)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var member = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    /// <summary>Update member profile</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<MemberDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Deactivate member (fails if they have future bookings)</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeactivateAsync(id);
        return NoContent();
    }

    /// <summary>Get member's bookings with optional filters</summary>
    [HttpGet("{id:int}/bookings")]
    [ProducesResponseType<PagedResult<BookingDto>>(200)]
    public async Task<IActionResult> GetBookings(
        int id, [FromQuery] string? status,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetMemberBookingsAsync(id, status, from, to, page, pageSize));

    /// <summary>Get member's upcoming confirmed bookings</summary>
    [HttpGet("{id:int}/bookings/upcoming")]
    [ProducesResponseType<IReadOnlyList<BookingDto>>(200)]
    public async Task<IActionResult> GetUpcomingBookings(int id)
        => Ok(await _service.GetUpcomingBookingsAsync(id));

    /// <summary>Get membership history for a member</summary>
    [HttpGet("{id:int}/memberships")]
    [ProducesResponseType<IReadOnlyList<MembershipDto>>(200)]
    public async Task<IActionResult> GetMemberships(int id)
        => Ok(await _service.GetMemberMembershipsAsync(id));
}
