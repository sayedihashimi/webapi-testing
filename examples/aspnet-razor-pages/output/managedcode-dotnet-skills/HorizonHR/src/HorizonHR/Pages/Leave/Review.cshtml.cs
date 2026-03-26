using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Leave;

public class ReviewModel(ILeaveService leaveService, IEmployeeService employeeService) : PageModel
{
    public new LeaveRequest? Request { get; set; }

    [BindProperty]
    public int ReviewerId { get; set; }

    [BindProperty]
    public string? ReviewNotes { get; set; }

    public List<SelectListItem> ReviewerOptions { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Request = await leaveService.GetRequestByIdAsync(id);
        if (Request is null) return NotFound();

        await LoadOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        try
        {
            await leaveService.ApproveAsync(id, ReviewerId, ReviewNotes);
            TempData["SuccessMessage"] = "Leave request approved.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            Request = await leaveService.GetRequestByIdAsync(id);
            await LoadOptionsAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        try
        {
            await leaveService.RejectAsync(id, ReviewerId, ReviewNotes);
            TempData["SuccessMessage"] = "Leave request rejected.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            Request = await leaveService.GetRequestByIdAsync(id);
            await LoadOptionsAsync();
            return Page();
        }
    }

    private async Task LoadOptionsAsync()
    {
        var (employees, _) = await employeeService.GetPagedAsync(1, 500, status: EmployeeStatus.Active);
        ReviewerOptions = employees.Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();
    }
}
