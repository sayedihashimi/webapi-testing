using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs.Prescription;
using VetClinicApi.Services.Interfaces;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;

    public PrescriptionsController(IPrescriptionService prescriptionService) => _prescriptionService = prescriptionService;

    /// <summary>Get prescription details</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PrescriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _prescriptionService.GetByIdAsync(id));

    /// <summary>Create a prescription for a medical record</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PrescriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionDto dto)
    {
        var result = await _prescriptionService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
