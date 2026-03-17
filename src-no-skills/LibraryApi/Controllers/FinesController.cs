using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinesController : ControllerBase
{
    private readonly FineService _service;

    public FinesController(FineService service) => _service = service;

    /// <summary>Get all fines with optional status filter and pagination.</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<FineDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetAllAsync(status, page, pageSize));

    /// <summary>Get fine by ID.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FineDto>> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Pay a fine.</summary>
    [HttpPost("{id}/pay")]
    public async Task<ActionResult<FineDto>> Pay(int id)
        => Ok(await _service.PayAsync(id));

    /// <summary>Waive a fine.</summary>
    [HttpPost("{id}/waive")]
    public async Task<ActionResult<FineDto>> Waive(int id)
        => Ok(await _service.WaiveAsync(id));
}
