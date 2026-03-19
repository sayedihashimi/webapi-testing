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

    public MembershipsController(IMembershipService service) => _service = service;

    /// <summary>Purchase/create a membership for a member</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MembershipDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateMembershipDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Get membership details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MembershipDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    /// <summary>Cancel a membership</summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(MembershipDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancel(int id) => Ok(await _service.CancelAsync(id));

    /// <summary>Freeze a membership (provide freeze duration 7-30 days)</summary>
    [HttpPost("{id}/freeze")]
    [ProducesResponseType(typeof(MembershipDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Freeze(int id, [FromBody] FreezeMembershipDto dto)
        => Ok(await _service.FreezeAsync(id, dto));

    /// <summary>Unfreeze a membership (extends EndDate by freeze duration)</summary>
    [HttpPost("{id}/unfreeze")]
    [ProducesResponseType(typeof(MembershipDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Unfreeze(int id) => Ok(await _service.UnfreezeAsync(id));

    /// <summary>Renew an expired or cancelled membership</summary>
    [HttpPost("{id}/renew")]
    [ProducesResponseType(typeof(MembershipDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Renew(int id)
    {
        var result = await _service.RenewAsync(id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
