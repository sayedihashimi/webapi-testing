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

    /// <summary>Record a new vaccination</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VaccinationResponseDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] VaccinationCreateDto dto)
    {
        var (result, error) = await _service.CreateAsync(dto);
        if (error is not null) return BadRequest(new ProblemDetails { Title = "Vaccination creation failed", Detail = error, Status = 400 });
        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    /// <summary>Get vaccination details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VaccinationResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var vax = await _service.GetByIdAsync(id);
        return vax is null ? NotFound() : Ok(vax);
    }
}
