using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Reviews;

public class CreateModel(IReviewService reviewService, IEmployeeService employeeService) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public List<SelectListItem> Employees { get; set; } = [];

    public class InputModel
    {
        [Required] public int EmployeeId { get; set; }
        [Required] public int ReviewerId { get; set; }
        [Required] public DateOnly ReviewPeriodStart { get; set; }
        [Required] public DateOnly ReviewPeriodEnd { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadEmployees();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await LoadEmployees(); return Page(); }
        if (Input.ReviewPeriodEnd <= Input.ReviewPeriodStart)
        {
            ModelState.AddModelError("Input.ReviewPeriodEnd", "End date must be after start date.");
            await LoadEmployees(); return Page();
        }

        var review = new PerformanceReview
        {
            EmployeeId = Input.EmployeeId,
            ReviewerId = Input.ReviewerId,
            ReviewPeriodStart = Input.ReviewPeriodStart,
            ReviewPeriodEnd = Input.ReviewPeriodEnd
        };

        var error = await reviewService.CreateAsync(review);
        if (error is not null)
        {
            ModelState.AddModelError("", error);
            await LoadEmployees(); return Page();
        }

        TempData["Success"] = "Performance review created.";
        return RedirectToPage("Details", new { id = review.Id });
    }

    private async Task LoadEmployees()
    {
        var emps = await employeeService.GetAllActiveAsync();
        Employees = emps.Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();
    }
}
