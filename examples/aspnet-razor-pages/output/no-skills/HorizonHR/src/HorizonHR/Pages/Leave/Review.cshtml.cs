using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Leave;

public class ReviewModel : PageModel
{
    private readonly ILeaveService _leaveService;
    private readonly IEmployeeService _employeeService;

    public ReviewModel(ILeaveService leaveService, IEmployeeService employeeService)
    {
        _leaveService = leaveService;
        _employeeService = employeeService;
    }

    public LeaveRequest? LeaveRequest { get; set; }
    public List<SelectListItem> ReviewerOptions { get; set; } = new();

    [BindProperty]
    public int ReviewedById { get; set; }

    [BindProperty]
    public string? ReviewNotes { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        LeaveRequest = await _leaveService.GetRequestByIdAsync(id);
        if (LeaveRequest == null) return NotFound();
        await LoadOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        try
        {
            await _leaveService.ApproveAsync(id, ReviewedById, ReviewNotes);
            TempData["Success"] = "Leave request approved.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            LeaveRequest = await _leaveService.GetRequestByIdAsync(id);
            await LoadOptionsAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        try
        {
            await _leaveService.RejectAsync(id, ReviewedById, ReviewNotes);
            TempData["Success"] = "Leave request rejected.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            LeaveRequest = await _leaveService.GetRequestByIdAsync(id);
            await LoadOptionsAsync();
            return Page();
        }
    }

    private async Task LoadOptionsAsync()
    {
        var employees = await _employeeService.GetAllActiveAsync();
        ReviewerOptions = employees.Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();
    }
}
