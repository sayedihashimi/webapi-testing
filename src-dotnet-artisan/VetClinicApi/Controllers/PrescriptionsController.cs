using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionsController(IPrescriptionService prescriptionService) : ControllerBase
{
    /// <summary>Get prescription details</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PrescriptionDto>> GetById(int id)
    {
        var prescription = await prescriptionService.GetByIdAsync(id);
        return prescription is null ? NotFound() : Ok(prescription);
    }

    /// <summary>Create a prescription for a medical record</summary>
    [HttpPost]
    public async Task<ActionResult<PrescriptionDto>> Create([FromBody] CreatePrescriptionDto dto)
    {
        var prescription = await prescriptionService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, prescription);
    }
}
