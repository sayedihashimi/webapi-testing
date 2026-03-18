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

    /// <summary>List all owners with optional search and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OwnerResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(search, new PaginationParams { Page = page, PageSize = pageSize }));

    /// <summary>Get owner by ID with their pets</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OwnerDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var owner = await _service.GetByIdAsync(id);
        return owner is null ? NotFound() : Ok(owner);
    }

    /// <summary>Create a new owner</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OwnerResponseDto), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] OwnerCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update an existing owner</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OwnerResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] OwnerUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Delete owner (fails if owner has active pets)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await _service.DeleteAsync(id);
        if (error == "Owner not found") return NotFound();
        if (!success) return BadRequest(new ProblemDetails { Title = "Cannot delete owner", Detail = error, Status = 400 });
        return NoContent();
    }

    /// <summary>Get all pets for an owner</summary>
    [HttpGet("{id}/pets")]
    [ProducesResponseType(typeof(IEnumerable<PetResponseDto>), 200)]
    public async Task<IActionResult> GetPets(int id)
        => Ok(await _service.GetPetsAsync(id));

    /// <summary>Get appointment history for an owner's pets</summary>
    [HttpGet("{id}/appointments")]
    [ProducesResponseType(typeof(PagedResult<AppointmentResponseDto>), 200)]
    public async Task<IActionResult> GetAppointments(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAppointmentsAsync(id, new PaginationParams { Page = page, PageSize = pageSize }));
}
