using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Leave;

public class CancelModel : PageModel
{
    private readonly ILeaveService _leaveService;

    public CancelModel(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    public LeaveRequest LeaveRequest { get; set; } = null!;
    public bool WasPreviouslyApproved { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var leaveRequest = await _leaveService.GetLeaveRequestByIdAsync(id);
        if (leaveRequest == null)
        {
            return NotFound();
        }

        LeaveRequest = leaveRequest;
        WasPreviouslyApproved = leaveRequest.Status == LeaveRequestStatus.Approved;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var leaveRequest = await _leaveService.GetLeaveRequestByIdAsync(id);
        if (leaveRequest == null)
        {
            return NotFound();
        }

        try
        {
            await _leaveService.CancelLeaveRequestAsync(id);
            TempData["SuccessMessage"] = "Leave request cancelled successfully.";
            return RedirectToPage("/Leave/Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            LeaveRequest = leaveRequest;
            WasPreviouslyApproved = leaveRequest.Status == LeaveRequestStatus.Approved;
            return Page();
        }
    }
}
