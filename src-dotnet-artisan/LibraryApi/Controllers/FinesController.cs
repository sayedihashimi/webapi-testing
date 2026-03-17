using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinesController : ControllerBase
{
    private readonly IFineService _service;

    public FinesController(IFineService service) => _service = service;

    /// <summary>List fines with optional status filter and pagination</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<FineDto>>> GetAll(
        [FromQuery] FineStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;
        return Ok(await _service.GetAllAsync(status, page, pageSize));
    }

    /// <summary>Get fine details</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FineDto>> GetById(int id)
    {
        var fine = await _service.GetByIdAsync(id);
        return fine is null ? NotFound() : Ok(fine);
    }

    /// <summary>Pay a fine</summary>
    [HttpPost("{id}/pay")]
    public async Task<ActionResult<FineDto>> Pay(int id)
    {
        return Ok(await _service.PayAsync(id));
    }

    /// <summary>Waive a fine</summary>
    [HttpPost("{id}/waive")]
    public async Task<ActionResult<FineDto>> Waive(int id)
    {
        return Ok(await _service.WaiveAsync(id));
    }
}
