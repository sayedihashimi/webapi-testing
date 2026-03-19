using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    private readonly IPetService _service;

    public PetsController(IPetService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<PetResponseDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? species,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(search, species, includeInactive, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PetResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
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
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

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
    {
        var result = await _service.GetMedicalRecordsAsync(id);
        return Ok(result);
    }

    [HttpGet("{id}/vaccinations")]
    [ProducesResponseType(typeof(List<VaccinationResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetVaccinations(int id)
    {
        var result = await _service.GetVaccinationsAsync(id);
        return Ok(result);
    }

    [HttpGet("{id}/vaccinations/upcoming")]
    [ProducesResponseType(typeof(List<VaccinationResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUpcomingVaccinations(int id)
    {
        var result = await _service.GetUpcomingVaccinationsAsync(id);
        return Ok(result);
    }

    [HttpGet("{id}/prescriptions/active")]
    [ProducesResponseType(typeof(List<PrescriptionResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetActivePrescriptions(int id)
    {
        var result = await _service.GetActivePrescriptionsAsync(id);
        return Ok(result);
    }
}
