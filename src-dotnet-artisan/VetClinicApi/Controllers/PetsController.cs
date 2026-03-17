using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetsController(IPetService petService) : ControllerBase
{
    /// <summary>List active pets with optional search and species filter</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<PetDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? species,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await petService.GetAllAsync(search, species, includeInactive, new PaginationParams(page, pageSize));
        return Ok(result);
    }

    /// <summary>Get pet details with owner info</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PetDetailDto>> GetById(int id)
    {
        var pet = await petService.GetByIdAsync(id);
        return pet is null ? NotFound() : Ok(pet);
    }

    /// <summary>Register a new pet</summary>
    [HttpPost]
    public async Task<ActionResult<PetDto>> Create([FromBody] CreatePetDto dto)
    {
        var pet = await petService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, pet);
    }

    /// <summary>Update pet info (including owner transfer)</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PetDto>> Update(int id, [FromBody] UpdatePetDto dto)
    {
        var pet = await petService.UpdateAsync(id, dto);
        return pet is null ? NotFound() : Ok(pet);
    }

    /// <summary>Soft-delete a pet (sets IsActive = false)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await petService.SoftDeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Get pet's medical records</summary>
    [HttpGet("{id:int}/medical-records")]
    public async Task<ActionResult<IReadOnlyList<MedicalRecordDto>>> GetMedicalRecords(int id)
    {
        var records = await petService.GetMedicalRecordsAsync(id);
        return Ok(records);
    }

    /// <summary>Get pet's vaccination history</summary>
    [HttpGet("{id:int}/vaccinations")]
    public async Task<ActionResult<IReadOnlyList<VaccinationDto>>> GetVaccinations(int id)
    {
        var vaccinations = await petService.GetVaccinationsAsync(id);
        return Ok(vaccinations);
    }

    /// <summary>Get upcoming/overdue vaccinations for a pet</summary>
    [HttpGet("{id:int}/vaccinations/upcoming")]
    public async Task<ActionResult<IReadOnlyList<VaccinationDto>>> GetUpcomingVaccinations(int id)
    {
        var vaccinations = await petService.GetUpcomingVaccinationsAsync(id);
        return Ok(vaccinations);
    }

    /// <summary>Get active prescriptions for a pet</summary>
    [HttpGet("{id:int}/prescriptions/active")]
    public async Task<ActionResult<IReadOnlyList<PrescriptionDto>>> GetActivePrescriptions(int id)
    {
        var prescriptions = await petService.GetActivePrescriptionsAsync(id);
        return Ok(prescriptions);
    }
}
