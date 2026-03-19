using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs.Common;
using VetClinicApi.DTOs.Pet;
using VetClinicApi.Services.Interfaces;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PetsController : ControllerBase
{
    private readonly IPetService _petService;

    public PetsController(IPetService petService) => _petService = petService;

    /// <summary>List active pets (optionally include inactive)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool includeInactive = false, [FromQuery] PaginationParams? pagination = null)
        => Ok(await _petService.GetAllAsync(search, includeInactive, pagination ?? new PaginationParams()));

    /// <summary>Get pet details with owner</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PetDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _petService.GetByIdAsync(id));

    /// <summary>Create a new pet</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePetDto dto)
    {
        var result = await _petService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a pet (including owner transfer)</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePetDto dto)
        => Ok(await _petService.UpdateAsync(id, dto));

    /// <summary>Soft-delete a pet</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _petService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get pet's medical records</summary>
    [HttpGet("{id:int}/medical-records")]
    [ProducesResponseType(typeof(List<DTOs.MedicalRecord.MedicalRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMedicalRecords(int id)
        => Ok(await _petService.GetMedicalRecordsAsync(id));

    /// <summary>Get pet's vaccinations</summary>
    [HttpGet("{id:int}/vaccinations")]
    [ProducesResponseType(typeof(List<DTOs.Vaccination.VaccinationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVaccinations(int id)
        => Ok(await _petService.GetVaccinationsAsync(id));

    /// <summary>Get pet's upcoming/overdue vaccinations</summary>
    [HttpGet("{id:int}/vaccinations/upcoming")]
    [ProducesResponseType(typeof(List<DTOs.Vaccination.VaccinationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUpcomingVaccinations(int id)
        => Ok(await _petService.GetUpcomingVaccinationsAsync(id));

    /// <summary>Get pet's active prescriptions</summary>
    [HttpGet("{id:int}/prescriptions/active")]
    [ProducesResponseType(typeof(List<DTOs.Prescription.PrescriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActivePrescriptions(int id)
        => Ok(await _petService.GetActivePrescriptionsAsync(id));
}
