using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Fine;
using LibraryApi.Models.Enums;
using LibraryApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FinesController : ControllerBase
{
    private readonly IFineService _service;

    public FinesController(IFineService service) => _service = service;

    /// <summary>List fines with filter, pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FineListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] FineStatus? status, [FromQuery] PaginationParams pagination)
        => Ok(await _service.GetAllAsync(status, pagination));

    /// <summary>Get fine details.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FineDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Pay a fine.</summary>
    [HttpPost("{id}/pay")]
    [ProducesResponseType(typeof(FineDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Pay(int id)
        => Ok(await _service.PayAsync(id));

    /// <summary>Waive a fine.</summary>
    [HttpPost("{id}/waive")]
    [ProducesResponseType(typeof(FineDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Waive(int id)
        => Ok(await _service.WaiveAsync(id));
}
