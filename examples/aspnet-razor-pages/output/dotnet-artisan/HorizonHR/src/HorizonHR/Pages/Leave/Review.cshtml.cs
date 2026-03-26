using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Leave;

public class ReviewModel(ILeaveService leaveService, IEmployeeService employeeService) : PageModel
{
    public LeaveRequest Request { get; set; } = null!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> Reviewers { get; set; } = [];

    public class InputModel
    {
        public int Id { get; set; }
        [Required] public int ReviewedById { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var req = await leaveService.GetRequestByIdAsync(id);
        if (req is null || req.Status != LeaveRequestStatus.Submitted) { return NotFound(); }
        Request = req;
        Input.Id = id;
        await LoadReviewers();
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync()
    {
        var req = await leaveService.GetRequestByIdAsync(Input.Id);
        if (req is null) { return NotFound(); }
        Request = req;

        if (!ModelState.IsValid) { await LoadReviewers(); return Page(); }

        var error = await leaveService.ApproveAsync(Input.Id, Input.ReviewedById, Input.Notes);
        if (error is not null)
        {
            ModelState.AddModelError("", error);
            await LoadReviewers();
            return Page();
        }

        TempData["Success"] = "Leave request approved.";
        return RedirectToPage("Details", new { id = Input.Id });
    }

    public async Task<IActionResult> OnPostRejectAsync()
    {
        var req = await leaveService.GetRequestByIdAsync(Input.Id);
        if (req is null) { return NotFound(); }
        Request = req;

        if (!ModelState.IsValid) { await LoadReviewers(); return Page(); }

        var error = await leaveService.RejectAsync(Input.Id, Input.ReviewedById, Input.Notes);
        if (error is not null)
        {
            ModelState.AddModelError("", error);
            await LoadReviewers();
            return Page();
        }

        TempData["Success"] = "Leave request rejected.";
        return RedirectToPage("Details", new { id = Input.Id });
    }

    private async Task LoadReviewers()
    {
        var emps = await employeeService.GetAllActiveAsync();
        Reviewers = emps.Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();
    }
}
