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

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OwnerResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetAllAsync(search, pagination));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OwnerResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OwnerResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateOwnerDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OwnerResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOwnerDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/pets")]
    [ProducesResponseType(typeof(List<PetResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPets(int id)
        => Ok(await _service.GetPetsAsync(id));

    [HttpGet("{id}/appointments")]
    [ProducesResponseType(typeof(PagedResponse<AppointmentResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAppointments(int id, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetAppointmentsAsync(id, pagination));
}
