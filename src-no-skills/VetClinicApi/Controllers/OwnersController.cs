using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs.Common;
using VetClinicApi.DTOs.Owner;
using VetClinicApi.Services.Interfaces;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OwnersController : ControllerBase
{
    private readonly IOwnerService _ownerService;

    public OwnersController(IOwnerService ownerService) => _ownerService = ownerService;

    /// <summary>List owners with optional search and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OwnerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] PaginationParams pagination)
        => Ok(await _ownerService.GetAllAsync(search, pagination));

    /// <summary>Get owner details with pets</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OwnerDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _ownerService.GetByIdAsync(id));

    /// <summary>Create a new owner</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOwnerDto dto)
    {
        var result = await _ownerService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update an owner</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOwnerDto dto)
        => Ok(await _ownerService.UpdateAsync(id, dto));

    /// <summary>Delete an owner (fails if has active pets)</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await _ownerService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get an owner's pets</summary>
    [HttpGet("{id:int}/pets")]
    [ProducesResponseType(typeof(List<DTOs.Pet.PetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPets(int id)
        => Ok(await _ownerService.GetPetsAsync(id));

    /// <summary>Get appointments for an owner's pets</summary>
    [HttpGet("{id:int}/appointments")]
    [ProducesResponseType(typeof(PagedResult<DTOs.Appointment.AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointments(int id, [FromQuery] PaginationParams pagination)
        => Ok(await _ownerService.GetAppointmentsAsync(id, pagination));
}
