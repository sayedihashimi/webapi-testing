using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    private readonly IPetService _service;

    public PetsController(IPetService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<PetResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? species,
        [FromQuery] bool includeInactive = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(search, species, includeInactive, new PaginationParams { Page = page, PageSize = pageSize }));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PetResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PetResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreatePetDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PetResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePetDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/medical-records")]
    [ProducesResponseType(typeof(List<MedicalRecordResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMedicalRecords(int id)
        => Ok(await _service.GetMedicalRecordsAsync(id));

    [HttpGet("{id}/vaccinations")]
    [ProducesResponseType(typeof(List<VaccinationResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetVaccinations(int id)
        => Ok(await _service.GetVaccinationsAsync(id));

    [HttpGet("{id}/vaccinations/upcoming")]
    [ProducesResponseType(typeof(List<VaccinationResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUpcomingVaccinations(int id)
        => Ok(await _service.GetUpcomingVaccinationsAsync(id));

    [HttpGet("{id}/prescriptions/active")]
    [ProducesResponseType(typeof(List<PrescriptionResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetActivePrescriptions(int id)
        => Ok(await _service.GetActivePrescriptionsAsync(id));
}
