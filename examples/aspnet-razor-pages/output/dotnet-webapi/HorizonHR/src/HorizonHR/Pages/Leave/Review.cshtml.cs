using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Leave;

public sealed class ReviewModel(ILeaveService leaveService, IEmployeeService employeeService) : PageModel
{
    public new LeaveRequest Request { get; set; } = null!;
    public List<SelectListItem> ReviewerList { get; set; } = [];

    [BindProperty]
    public int ReviewerId { get; set; }

    [BindProperty]
    public string? ReviewNotes { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var req = await leaveService.GetRequestByIdAsync(id, ct);
        if (req == null || req.Status != LeaveRequestStatus.Submitted) return NotFound();

        Request = req;
        await LoadReviewersAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id, CancellationToken ct)
    {
        try
        {
            await leaveService.ApproveAsync(id, ReviewerId, ReviewNotes, ct);
            TempData["SuccessMessage"] = "Leave request approved.";
            return RedirectToPage("Details", new { id });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("Details", new { id });
        }
    }

    public async Task<IActionResult> OnPostRejectAsync(int id, CancellationToken ct)
    {
        try
        {
            await leaveService.RejectAsync(id, ReviewerId, ReviewNotes, ct);
            TempData["SuccessMessage"] = "Leave request rejected.";
            return RedirectToPage("Details", new { id });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("Details", new { id });
        }
    }

    private async Task LoadReviewersAsync(CancellationToken ct)
    {
        var employees = await employeeService.GetAllAsync(null, null, null, EmployeeStatus.Active, 1, 1000, ct);
        ReviewerList = employees.Items.Select(e => new SelectListItem($"{e.FirstName} {e.LastName}", e.Id.ToString())).ToList();
    }
}
