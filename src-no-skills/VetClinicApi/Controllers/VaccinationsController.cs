using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VaccinationsController : ControllerBase
{
    private readonly IVaccinationService _service;

    public VaccinationsController(IVaccinationService service) => _service = service;

    /// <summary>Get vaccination by ID</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VaccinationDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    /// <summary>Create a new vaccination record</summary>
    [HttpPost]
    public async Task<ActionResult<VaccinationDto>> Create([FromBody] CreateVaccinationDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
