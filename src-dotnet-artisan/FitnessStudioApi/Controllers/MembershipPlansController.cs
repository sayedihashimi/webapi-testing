using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/membership-plans")]
public class MembershipPlansController(IMembershipPlanService service) : ControllerBase
{
    /// <summary>List all active membership plans</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MembershipPlanDto>>> GetAll()
        => Ok(await service.GetAllActiveAsync());

    /// <summary>Get membership plan by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MembershipPlanDto>> GetById(int id)
    {
        var plan = await service.GetByIdAsync(id);
        return plan is null ? NotFound() : Ok(plan);
    }

    /// <summary>Create a new membership plan</summary>
    [HttpPost]
    public async Task<ActionResult<MembershipPlanDto>> Create(CreateMembershipPlanDto dto)
    {
        var plan = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    /// <summary>Update a membership plan</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MembershipPlanDto>> Update(int id, UpdateMembershipPlanDto dto)
    {
        var plan = await service.UpdateAsync(id, dto);
        return plan is null ? NotFound() : Ok(plan);
    }

    /// <summary>Deactivate a membership plan</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.DeactivateAsync(id);
        return result ? NoContent() : NotFound();
    }
}
