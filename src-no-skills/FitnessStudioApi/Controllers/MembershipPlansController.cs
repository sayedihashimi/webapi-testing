using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/membership-plans")]
[Produces("application/json")]
public class MembershipPlansController : ControllerBase
{
    private readonly IMembershipPlanService _service;

    public MembershipPlansController(IMembershipPlanService service)
    {
        _service = service;
    }

    /// <summary>List all active membership plans</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MembershipPlanResponseDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var plans = await _service.GetAllActiveAsync();
        return Ok(plans);
    }

    /// <summary>Get membership plan by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MembershipPlanResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var plan = await _service.GetByIdAsync(id);
        return plan is null ? NotFound() : Ok(plan);
    }

    /// <summary>Create a new membership plan</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MembershipPlanResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] MembershipPlanCreateDto dto)
    {
        var plan = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    /// <summary>Update a membership plan</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MembershipPlanResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int id, [FromBody] MembershipPlanUpdateDto dto)
    {
        var plan = await _service.UpdateAsync(id, dto);
        return plan is null ? NotFound() : Ok(plan);
    }

    /// <summary>Deactivate a membership plan</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeactivateAsync(id);
        return result ? NoContent() : NotFound();
    }
}
