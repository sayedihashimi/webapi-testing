using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/medical-records")]
public class MedicalRecordsController(IMedicalRecordService medicalRecordService) : ControllerBase
{
    /// <summary>Get medical record details with prescriptions</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MedicalRecordDetailDto>> GetById(int id)
    {
        var record = await medicalRecordService.GetByIdAsync(id);
        return record is null ? NotFound() : Ok(record);
    }

    /// <summary>Create a medical record (only for completed/in-progress appointments)</summary>
    [HttpPost]
    public async Task<ActionResult<MedicalRecordDto>> Create([FromBody] CreateMedicalRecordDto dto)
    {
        var record = await medicalRecordService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    /// <summary>Update a medical record</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MedicalRecordDto>> Update(int id, [FromBody] UpdateMedicalRecordDto dto)
    {
        var record = await medicalRecordService.UpdateAsync(id, dto);
        return record is null ? NotFound() : Ok(record);
    }
}
