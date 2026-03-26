using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Leave;

public class DetailsModel : PageModel
{
    private readonly ILeaveService _leaveService;

    public DetailsModel(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    public LeaveRequest? LeaveRequest { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        LeaveRequest = await _leaveService.GetRequestByIdAsync(id);
        if (LeaveRequest == null) return NotFound();
        return Page();
    }
}
