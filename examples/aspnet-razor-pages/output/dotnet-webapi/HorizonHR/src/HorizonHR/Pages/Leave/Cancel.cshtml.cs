using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Leave;

public sealed class CancelModel(ILeaveService leaveService) : PageModel
{
    public new LeaveRequest Request { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var req = await leaveService.GetRequestByIdAsync(id, ct);
        if (req == null || (req.Status != LeaveRequestStatus.Submitted && req.Status != LeaveRequestStatus.Approved))
            return NotFound();

        Request = req;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        try
        {
            await leaveService.CancelAsync(id, ct);
            TempData["SuccessMessage"] = "Leave request cancelled.";
            return RedirectToPage("Details", new { id });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("Details", new { id });
        }
    }
}
