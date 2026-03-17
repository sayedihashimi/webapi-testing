using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OwnersController(IOwnerService ownerService) : ControllerBase
{
    /// <summary>List owners with optional search by name or email</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<OwnerDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await ownerService.GetAllAsync(search, new PaginationParams(page, pageSize));
        return Ok(result);
    }

    /// <summary>Get owner details including pets</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OwnerDetailDto>> GetById(int id)
    {
        var owner = await ownerService.GetByIdAsync(id);
        return owner is null ? NotFound() : Ok(owner);
    }

    /// <summary>Create a new owner</summary>
    [HttpPost]
    public async Task<ActionResult<OwnerDto>> Create([FromBody] CreateOwnerDto dto)
    {
        var owner = await ownerService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = owner.Id }, owner);
    }

    /// <summary>Update an existing owner</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<OwnerDto>> Update(int id, [FromBody] UpdateOwnerDto dto)
    {
        var owner = await ownerService.UpdateAsync(id, dto);
        return owner is null ? NotFound() : Ok(owner);
    }

    /// <summary>Delete an owner (fails if owner has active pets)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await ownerService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Get all pets belonging to this owner</summary>
    [HttpGet("{id:int}/pets")]
    public async Task<ActionResult<IReadOnlyList<PetDto>>> GetPets(int id)
    {
        var pets = await ownerService.GetPetsAsync(id);
        return Ok(pets);
    }

    /// <summary>Get appointments for all of this owner's pets</summary>
    [HttpGet("{id:int}/appointments")]
    public async Task<ActionResult<PagedResult<AppointmentDto>>> GetAppointments(
        int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await ownerService.GetAppointmentsAsync(id, new PaginationParams(page, pageSize));
        return Ok(result);
    }
}
