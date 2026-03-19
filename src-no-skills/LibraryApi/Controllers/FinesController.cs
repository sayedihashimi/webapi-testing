using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FinesController : ControllerBase
{
    private readonly IFineService _service;

    public FinesController(IFineService service) => _service = service;

    /// <summary>List fines with filter by status and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FineDto>), 200)]
    public async Task<IActionResult> GetFines([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetFinesAsync(status, page, pageSize));

    /// <summary>Get fine details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FineDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetFine(int id)
        => Ok(await _service.GetFineByIdAsync(id));

    /// <summary>Pay a fine</summary>
    [HttpPost("{id}/pay")]
    [ProducesResponseType(typeof(FineDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PayFine(int id)
        => Ok(await _service.PayFineAsync(id));

    /// <summary>Waive a fine</summary>
    [HttpPost("{id}/waive")]
    [ProducesResponseType(typeof(FineDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> WaiveFine(int id)
        => Ok(await _service.WaiveFineAsync(id));
}
