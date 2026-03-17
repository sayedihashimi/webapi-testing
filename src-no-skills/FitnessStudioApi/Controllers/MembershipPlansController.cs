using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/membership-plans")]
[Produces("application/json")]
public class MembershipPlansController : ControllerBase
{
    private readonly MembershipPlanService _service;

    public MembershipPlansController(MembershipPlanService service)
    {
        _service = service;
    }

    /// <summary>Get all membership plans</summary>
    [HttpGet]
    public async Task<ActionResult<List<MembershipPlanDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    /// <summary>Get a membership plan by ID</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MembershipPlanDto>> GetById(int id)
    {
        var plan = await _service.GetByIdAsync(id);
        return plan == null ? NotFound() : Ok(plan);
    }

    /// <summary>Create a new membership plan</summary>
    [HttpPost]
    public async Task<ActionResult<MembershipPlanDto>> Create(CreateMembershipPlanDto dto)
    {
        var plan = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    /// <summary>Update a membership plan</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<MembershipPlanDto>> Update(int id, UpdateMembershipPlanDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Deactivate a membership plan</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
