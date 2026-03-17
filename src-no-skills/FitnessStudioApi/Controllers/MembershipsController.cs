using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/memberships")]
[Produces("application/json")]
public class MembershipsController : ControllerBase
{
    private readonly MembershipService _service;

    public MembershipsController(MembershipService service)
    {
        _service = service;
    }

    /// <summary>Create a new membership</summary>
    [HttpPost]
    public async Task<ActionResult<MembershipDto>> Create(CreateMembershipDto dto)
    {
        var ms = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = ms.Id }, ms);
    }

    /// <summary>Get a membership by ID</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MembershipDto>> GetById(int id)
    {
        var ms = await _service.GetByIdAsync(id);
        return ms == null ? NotFound() : Ok(ms);
    }

    /// <summary>Cancel a membership</summary>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<MembershipDto>> Cancel(int id)
        => Ok(await _service.CancelAsync(id));

    /// <summary>Freeze a membership (7-30 days)</summary>
    [HttpPost("{id}/freeze")]
    public async Task<ActionResult<MembershipDto>> Freeze(int id, FreezeMembershipDto dto)
        => Ok(await _service.FreezeAsync(id, dto));

    /// <summary>Unfreeze a membership</summary>
    [HttpPost("{id}/unfreeze")]
    public async Task<ActionResult<MembershipDto>> Unfreeze(int id)
        => Ok(await _service.UnfreezeAsync(id));

    /// <summary>Renew an expired or cancelled membership</summary>
    [HttpPost("{id}/renew")]
    public async Task<ActionResult<MembershipDto>> Renew(int id)
        => Ok(await _service.RenewAsync(id));
}
