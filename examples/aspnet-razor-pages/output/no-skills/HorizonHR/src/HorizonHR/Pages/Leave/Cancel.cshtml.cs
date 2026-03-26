using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Leave;

public class CancelModel : PageModel
{
    private readonly ILeaveService _leaveService;

    public CancelModel(ILeaveService leaveService)
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

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await _leaveService.CancelAsync(id);
            TempData["Success"] = "Leave request cancelled.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            LeaveRequest = await _leaveService.GetRequestByIdAsync(id);
            return Page();
        }
    }
}
