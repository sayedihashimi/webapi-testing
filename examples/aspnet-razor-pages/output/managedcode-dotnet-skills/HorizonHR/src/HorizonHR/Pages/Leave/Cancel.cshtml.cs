using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Leave;

public class CancelModel(ILeaveService leaveService) : PageModel
{
    public new LeaveRequest? Request { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Request = await leaveService.GetRequestByIdAsync(id);
        if (Request is null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await leaveService.CancelAsync(id);
            TempData["SuccessMessage"] = "Leave request cancelled.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            Request = await leaveService.GetRequestByIdAsync(id);
            return Page();
        }
    }
}
