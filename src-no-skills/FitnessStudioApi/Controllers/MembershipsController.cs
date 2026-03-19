using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType(typeof(MembershipResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] MembershipCreateDto dto)
    {
        var membership = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = membership.Id }, membership);
    }

    /// <summary>Get membership details</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MembershipResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var membership = await _service.GetByIdAsync(id);
        return membership is null ? NotFound() : Ok(membership);
    }

    /// <summary>Cancel a membership</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(MembershipResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _service.CancelAsync(id);
        return Ok(result);
    }

    /// <summary>Freeze a membership (7–30 days)</summary>
    [HttpPost("{id:int}/freeze")]
    [ProducesResponseType(typeof(MembershipResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Freeze(int id, [FromBody] FreezeMembershipDto dto)
    {
        var result = await _service.FreezeAsync(id, dto);
        return Ok(result);
    }

    /// <summary>Unfreeze a membership (extends end date by freeze duration)</summary>
    [HttpPost("{id:int}/unfreeze")]
    [ProducesResponseType(typeof(MembershipResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Unfreeze(int id)
    {
        var result = await _service.UnfreezeAsync(id);
        return Ok(result);
    }

    /// <summary>Renew an expired membership</summary>
    [HttpPost("{id:int}/renew")]
    [ProducesResponseType(typeof(MembershipResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Renew(int id)
    {
        var result = await _service.RenewAsync(id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
