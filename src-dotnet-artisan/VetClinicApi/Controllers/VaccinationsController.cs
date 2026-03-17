using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VaccinationsController(IVaccinationService vaccinationService) : ControllerBase
{
    /// <summary>Get vaccination details</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VaccinationDto>> GetById(int id)
    {
        var vaccination = await vaccinationService.GetByIdAsync(id);
        return vaccination is null ? NotFound() : Ok(vaccination);
    }

    /// <summary>Record a new vaccination</summary>
    [HttpPost]
    public async Task<ActionResult<VaccinationDto>> Create([FromBody] CreateVaccinationDto dto)
    {
        var vaccination = await vaccinationService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = vaccination.Id }, vaccination);
    }
}
