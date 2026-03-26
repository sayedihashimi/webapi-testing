using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Leave;

public class CancelModel(ILeaveService leaveService) : PageModel
{
    public LeaveRequest Request { get; set; } = null!;
    [BindProperty] public int Id { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var req = await leaveService.GetRequestByIdAsync(id);
        if (req is null || req.Status is LeaveRequestStatus.Cancelled or LeaveRequestStatus.Rejected)
        { return NotFound(); }
        Request = req;
        Id = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var error = await leaveService.CancelAsync(Id);
        if (error is not null)
        {
            TempData["Error"] = error;
            return RedirectToPage("Details", new { id = Id });
        }
        TempData["Success"] = "Leave request cancelled.";
        return RedirectToPage("Details", new { id = Id });
    }
}
