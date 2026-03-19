using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/membership-plans")]
[Produces("application/json")]
public class MembershipPlansController : ControllerBase
{
    private readonly IMembershipPlanService _service;

    public MembershipPlansController(IMembershipPlanService service) => _service = service;

    /// <summary>List all active membership plans</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MembershipPlanDto>), 200)]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    /// <summary>Get membership plan details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MembershipPlanDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    /// <summary>Create a new membership plan</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MembershipPlanDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateMembershipPlanDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a membership plan</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MembershipPlanDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMembershipPlanDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Deactivate a membership plan</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
