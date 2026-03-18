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

    /// <summary>List fines with filter by status and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<FineDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _fineService.GetAllAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get fine details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var fine = await _fineService.GetByIdAsync(id);
        if (fine == null) return NotFound(new ProblemDetails { Title = "Fine not found", Status = 404 });
        return Ok(fine);
    }

    /// <summary>Pay a fine (set PaidDate, update status to Paid)</summary>
    [HttpPost("{id}/pay")]
    [ProducesResponseType(typeof(FineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pay(int id)
    {
        var (fine, error) = await _fineService.PayAsync(id);
        if (fine == null)
        {
            if (error == "Fine not found.")
                return NotFound(new ProblemDetails { Title = "Fine not found", Status = 404 });
            return BadRequest(new ProblemDetails { Title = "Payment failed", Detail = error, Status = 400 });
        }
        return Ok(fine);
    }

    /// <summary>Waive a fine (update status to Waived)</summary>
    [HttpPost("{id}/waive")]
    [ProducesResponseType(typeof(FineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Waive(int id)
    {
        var (fine, error) = await _fineService.WaiveAsync(id);
        if (fine == null)
        {
            if (error == "Fine not found.")
                return NotFound(new ProblemDetails { Title = "Fine not found", Status = 404 });
            return BadRequest(new ProblemDetails { Title = "Waive failed", Detail = error, Status = 400 });
        }
        return Ok(fine);
    }
}
