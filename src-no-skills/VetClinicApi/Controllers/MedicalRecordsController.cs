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

    /// <summary>Get medical record with prescriptions</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MedicalRecordResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var record = await _service.GetByIdAsync(id);
        return record is null ? NotFound() : Ok(record);
    }

    /// <summary>Create medical record (appointment must be Completed or InProgress)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MedicalRecordResponseDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] MedicalRecordCreateDto dto)
    {
        var (result, error) = await _service.CreateAsync(dto);
        if (error is not null) return BadRequest(new ProblemDetails { Title = "Medical record creation failed", Detail = error, Status = 400 });
        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    /// <summary>Update medical record</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MedicalRecordResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] MedicalRecordUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }
}
