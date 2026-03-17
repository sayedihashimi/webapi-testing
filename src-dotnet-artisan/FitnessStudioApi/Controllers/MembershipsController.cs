using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/memberships")]
public class MembershipsController(IMembershipService service) : ControllerBase
{
    /// <summary>Get membership details</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MembershipDto>> GetById(int id)
    {
        var ms = await service.GetByIdAsync(id);
        return ms is null ? NotFound() : Ok(ms);
    }

    /// <summary>Create a new membership</summary>
    [HttpPost]
    public async Task<ActionResult<MembershipDto>> Create(CreateMembershipDto dto)
    {
        var (result, error) = await service.CreateAsync(dto);
        if (result is null) return BadRequest(new { error });
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Cancel a membership</summary>
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, error) = await service.CancelAsync(id);
        if (!success) return error == "Membership not found." ? NotFound() : BadRequest(new { error });
        return Ok(new { message = "Membership cancelled." });
    }

    /// <summary>Freeze a membership (7-30 days)</summary>
    [HttpPost("{id:int}/freeze")]
    public async Task<IActionResult> Freeze(int id, FreezeMembershipDto dto)
    {
        var (success, error) = await service.FreezeAsync(id, dto);
        if (!success) return error == "Membership not found." ? NotFound() : BadRequest(new { error });
        return Ok(new { message = "Membership frozen." });
    }

    /// <summary>Unfreeze a membership</summary>
    [HttpPost("{id:int}/unfreeze")]
    public async Task<IActionResult> Unfreeze(int id)
    {
        var (success, error) = await service.UnfreezeAsync(id);
        if (!success) return error == "Membership not found." ? NotFound() : BadRequest(new { error });
        return Ok(new { message = "Membership unfrozen." });
    }

    /// <summary>Renew an expired membership</summary>
    [HttpPost("{id:int}/renew")]
    public async Task<ActionResult<MembershipDto>> Renew(int id)
    {
        var (result, error) = await service.RenewAsync(id);
        if (result is null) return error == "Membership not found." ? NotFound() : BadRequest(new { error });
        return Ok(result);
    }
}
