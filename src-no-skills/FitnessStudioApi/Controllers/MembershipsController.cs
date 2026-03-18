using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType<MembershipDto>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMembershipDto dto,
        [FromServices] IValidator<CreateMembershipDto> validator)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var membership = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = membership.Id }, membership);
    }

    /// <summary>Get membership details</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<MembershipDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var membership = await _service.GetByIdAsync(id);
        return membership is null ? NotFound() : Ok(membership);
    }

    /// <summary>Cancel a membership</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType<MembershipDto>(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Cancel(int id)
        => Ok(await _service.CancelAsync(id));

    /// <summary>Freeze a membership (7-30 days)</summary>
    [HttpPost("{id:int}/freeze")]
    [ProducesResponseType<MembershipDto>(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Freeze(int id, [FromBody] FreezeMembershipDto dto,
        [FromServices] IValidator<FreezeMembershipDto> validator)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        return Ok(await _service.FreezeAsync(id, dto));
    }

    /// <summary>Unfreeze a membership (extends end date)</summary>
    [HttpPost("{id:int}/unfreeze")]
    [ProducesResponseType<MembershipDto>(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Unfreeze(int id)
        => Ok(await _service.UnfreezeAsync(id));

    /// <summary>Renew an expired or cancelled membership</summary>
    [HttpPost("{id:int}/renew")]
    [ProducesResponseType<MembershipDto>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Renew(int id)
    {
        var membership = await _service.RenewAsync(id);
        return CreatedAtAction(nameof(GetById), new { id = membership.Id }, membership);
    }
}
