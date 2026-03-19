using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs.Vaccination;
using VetClinicApi.Services.Interfaces;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VaccinationsController : ControllerBase
{
    private readonly IVaccinationService _vaccinationService;

    public VaccinationsController(IVaccinationService vaccinationService) => _vaccinationService = vaccinationService;

    /// <summary>Get vaccination details</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VaccinationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _vaccinationService.GetByIdAsync(id));

    /// <summary>Record a new vaccination</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VaccinationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateVaccinationDto dto)
    {
        var result = await _vaccinationService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
