using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/medical-records")]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordService _service;

    public MedicalRecordsController(IMedicalRecordService service) => _service = service;

    /// <summary>Get medical record by ID with prescriptions</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MedicalRecordDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    /// <summary>Create a medical record (appointment must be Completed or InProgress)</summary>
    [HttpPost]
    public async Task<ActionResult<MedicalRecordDto>> Create([FromBody] CreateMedicalRecordDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a medical record</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<MedicalRecordDto>> Update(int id, [FromBody] UpdateMedicalRecordDto dto)
    {
        return Ok(await _service.UpdateAsync(id, dto));
    }
}
