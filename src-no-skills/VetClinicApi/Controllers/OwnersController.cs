using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OwnersController : ControllerBase
{
    private readonly IOwnerService _service;

    public OwnersController(IOwnerService service) => _service = service;

    /// <summary>Get all owners with optional search and pagination</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<OwnerSummaryDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        return Ok(await _service.GetAllAsync(search, page, pageSize));
    }

    /// <summary>Get owner by ID including their pets</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OwnerDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    /// <summary>Create a new owner</summary>
    [HttpPost]
    public async Task<ActionResult<OwnerDto>> Create([FromBody] CreateOwnerDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update an existing owner</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<OwnerDto>> Update(int id, [FromBody] UpdateOwnerDto dto)
    {
        return Ok(await _service.UpdateAsync(id, dto));
    }

    /// <summary>Delete an owner (fails if they have active pets)</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get all pets belonging to an owner</summary>
    [HttpGet("{id}/pets")]
    public async Task<ActionResult<List<PetSummaryDto>>> GetPets(int id)
    {
        return Ok(await _service.GetPetsAsync(id));
    }

    /// <summary>Get all appointments for an owner's pets</summary>
    [HttpGet("{id}/appointments")]
    public async Task<ActionResult<PaginatedResponse<AppointmentSummaryDto>>> GetAppointments(
        int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        return Ok(await _service.GetAppointmentsAsync(id, page, pageSize));
    }
}
