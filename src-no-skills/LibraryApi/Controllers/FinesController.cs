using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FinesController : ControllerBase
{
    private readonly IFineService _fineService;

    public FinesController(IFineService fineService)
    {
        _fineService = fineService;
    }

    /// <summary>List fines with optional status filter and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FineDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FineDto>>> GetFines(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _fineService.GetFinesAsync(status,
            new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get fine details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FineDto>> GetFine(int id)
    {
        var result = await _fineService.GetFineByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Pay a fine</summary>
    [HttpPost("{id}/pay")]
    [ProducesResponseType(typeof(FineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FineDto>> PayFine(int id)
    {
        var result = await _fineService.PayFineAsync(id);
        return Ok(result);
    }

    /// <summary>Waive a fine</summary>
    [HttpPost("{id}/waive")]
    [ProducesResponseType(typeof(FineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FineDto>> WaiveFine(int id)
    {
        var result = await _fineService.WaiveFineAsync(id);
        return Ok(result);
    }
}
