using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs.MedicalRecord;
using VetClinicApi.Services.Interfaces;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/medical-records")]
[Produces("application/json")]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordService _medicalRecordService;

    public MedicalRecordsController(IMedicalRecordService medicalRecordService) => _medicalRecordService = medicalRecordService;

    /// <summary>Get medical record with prescriptions</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MedicalRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _medicalRecordService.GetByIdAsync(id));

    /// <summary>Create a medical record (appointment must be InProgress or Completed)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MedicalRecordDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateMedicalRecordDto dto)
    {
        var result = await _medicalRecordService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a medical record</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MedicalRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicalRecordDto dto)
        => Ok(await _medicalRecordService.UpdateAsync(id, dto));
}
