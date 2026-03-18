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

    /// <summary>List all active pets with optional search, species filter, and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PetResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? species, [FromQuery] bool includeInactive = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(search, species, includeInactive, new PaginationParams { Page = page, PageSize = pageSize }));

    /// <summary>Get pet by ID with owner info</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PetDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var pet = await _service.GetByIdAsync(id);
        return pet is null ? NotFound() : Ok(pet);
    }

    /// <summary>Create a new pet</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PetResponseDto), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] PetCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update pet (including owner transfer)</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PetResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] PetUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Soft-delete a pet (sets IsActive = false)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.SoftDeleteAsync(id);
        return success ? NoContent() : NotFound();
    }

    /// <summary>Get all medical records for a pet</summary>
    [HttpGet("{id}/medical-records")]
    [ProducesResponseType(typeof(IEnumerable<MedicalRecordResponseDto>), 200)]
    public async Task<IActionResult> GetMedicalRecords(int id)
        => Ok(await _service.GetMedicalRecordsAsync(id));

    /// <summary>Get all vaccinations for a pet</summary>
    [HttpGet("{id}/vaccinations")]
    [ProducesResponseType(typeof(IEnumerable<VaccinationResponseDto>), 200)]
    public async Task<IActionResult> GetVaccinations(int id)
        => Ok(await _service.GetVaccinationsAsync(id));

    /// <summary>Get vaccinations that are due soon or overdue for a pet</summary>
    [HttpGet("{id}/vaccinations/upcoming")]
    [ProducesResponseType(typeof(IEnumerable<VaccinationResponseDto>), 200)]
    public async Task<IActionResult> GetUpcomingVaccinations(int id)
        => Ok(await _service.GetUpcomingVaccinationsAsync(id));

    /// <summary>Get active prescriptions for a pet</summary>
    [HttpGet("{id}/prescriptions/active")]
    [ProducesResponseType(typeof(IEnumerable<PrescriptionResponseDto>), 200)]
    public async Task<IActionResult> GetActivePrescriptions(int id)
        => Ok(await _service.GetActivePrescriptionsAsync(id));
}
