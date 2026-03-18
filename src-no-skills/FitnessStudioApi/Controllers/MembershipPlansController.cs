using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType<IReadOnlyList<MembershipPlanDto>>(200)]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllActiveAsync());

    /// <summary>Get membership plan by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<MembershipPlanDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var plan = await _service.GetByIdAsync(id);
        return plan is null ? NotFound() : Ok(plan);
    }

    /// <summary>Create a new membership plan</summary>
    [HttpPost]
    [ProducesResponseType<MembershipPlanDto>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMembershipPlanDto dto,
        [FromServices] IValidator<CreateMembershipPlanDto> validator)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var plan = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    /// <summary>Update a membership plan</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<MembershipPlanDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMembershipPlanDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Deactivate a membership plan</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeactivateAsync(id);
        return NoContent();
    }
}
