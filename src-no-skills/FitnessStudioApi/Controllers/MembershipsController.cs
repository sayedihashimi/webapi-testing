using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/memberships")]
[Produces("application/json")]
public class MembershipsController : ControllerBase
{
    private readonly IMembershipService _service;

    public MembershipsController(IMembershipService service)
    {
        _service = service;
    }

    /// <summary>Purchase/create a membership for a member</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MembershipDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create([FromBody] CreateMembershipDto dto)
    {
        var membership = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = membership.Id }, membership);
    }

    /// <summary>Get membership details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MembershipDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var membership = await _service.GetByIdAsync(id);
        return Ok(membership);
    }

    /// <summary>Cancel a membership</summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(MembershipDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancel(int id)
    {
        var membership = await _service.CancelAsync(id);
        return Ok(membership);
    }

    /// <summary>Freeze a membership (7-30 days)</summary>
    [HttpPost("{id}/freeze")]
    [ProducesResponseType(typeof(MembershipDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Freeze(int id, [FromBody] FreezeMembershipDto dto)
    {
        var membership = await _service.FreezeAsync(id, dto);
        return Ok(membership);
    }

    /// <summary>Unfreeze a membership (extends EndDate)</summary>
    [HttpPost("{id}/unfreeze")]
    [ProducesResponseType(typeof(MembershipDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Unfreeze(int id)
    {
        var membership = await _service.UnfreezeAsync(id);
        return Ok(membership);
    }

    /// <summary>Renew an expired or cancelled membership</summary>
    [HttpPost("{id}/renew")]
    [ProducesResponseType(typeof(MembershipDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Renew(int id)
    {
        var membership = await _service.RenewAsync(id);
        return CreatedAtAction(nameof(GetById), new { id = membership.Id }, membership);
    }
}
