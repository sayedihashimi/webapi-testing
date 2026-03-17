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

    /// <summary>Get all pets with optional search, species filter, and pagination</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<PetDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? species,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        return Ok(await _service.GetAllAsync(search, species, includeInactive, page, pageSize));
    }

    /// <summary>Get pet by ID including owner details</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PetDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    /// <summary>Create a new pet</summary>
    [HttpPost]
    public async Task<ActionResult<PetDto>> Create([FromBody] CreatePetDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a pet</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<PetDto>> Update(int id, [FromBody] UpdatePetDto dto)
    {
        return Ok(await _service.UpdateAsync(id, dto));
    }

    /// <summary>Soft delete a pet (sets IsActive to false)</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get medical records for a pet</summary>
    [HttpGet("{id}/medical-records")]
    public async Task<ActionResult<List<MedicalRecordDto>>> GetMedicalRecords(int id)
    {
        return Ok(await _service.GetMedicalRecordsAsync(id));
    }

    /// <summary>Get all vaccinations for a pet</summary>
    [HttpGet("{id}/vaccinations")]
    public async Task<ActionResult<List<VaccinationDto>>> GetVaccinations(int id)
    {
        return Ok(await _service.GetVaccinationsAsync(id));
    }

    /// <summary>Get upcoming and overdue vaccinations for a pet</summary>
    [HttpGet("{id}/vaccinations/upcoming")]
    public async Task<ActionResult<List<VaccinationDto>>> GetUpcomingVaccinations(int id)
    {
        return Ok(await _service.GetUpcomingVaccinationsAsync(id));
    }

    /// <summary>Get active prescriptions for a pet</summary>
    [HttpGet("{id}/prescriptions/active")]
    public async Task<ActionResult<List<PrescriptionDto>>> GetActivePrescriptions(int id)
    {
        return Ok(await _service.GetActivePrescriptionsAsync(id));
    }
}
