using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _service;

    public PrescriptionsController(IPrescriptionService service) => _service = service;

    /// <summary>Get prescription details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PrescriptionResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var rx = await _service.GetByIdAsync(id);
        return rx is null ? NotFound() : Ok(rx);
    }

    /// <summary>Create a prescription for a medical record</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PrescriptionResponseDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] PrescriptionCreateDto dto)
    {
        var (result, error) = await _service.CreateAsync(dto);
        if (error is not null) return BadRequest(new ProblemDetails { Title = "Prescription creation failed", Detail = error, Status = 400 });
        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }
}
