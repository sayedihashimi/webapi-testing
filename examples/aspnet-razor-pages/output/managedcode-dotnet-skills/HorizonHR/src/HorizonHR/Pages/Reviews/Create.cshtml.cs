using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Reviews;

public class CreateModel(IReviewService reviewService, IEmployeeService employeeService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> EmployeeOptions { get; set; } = [];
    public List<SelectListItem> ReviewerOptions { get; set; } = [];

    public class InputModel
    {
        [Required, Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required, Display(Name = "Reviewer")]
        public int ReviewerId { get; set; }

        [Required, Display(Name = "Review Period Start")]
        public DateOnly ReviewPeriodStart { get; set; } = new(DateTime.Now.Year, 1, 1);

        [Required, Display(Name = "Review Period End")]
        public DateOnly ReviewPeriodEnd { get; set; } = new(DateTime.Now.Year, 12, 31);
    }

    public async Task OnGetAsync()
    {
        await LoadOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync();
            return Page();
        }

        try
        {
            var review = new PerformanceReview
            {
                EmployeeId = Input.EmployeeId,
                ReviewerId = Input.ReviewerId,
                ReviewPeriodStart = Input.ReviewPeriodStart,
                ReviewPeriodEnd = Input.ReviewPeriodEnd
            };

            await reviewService.CreateAsync(review);
            TempData["SuccessMessage"] = "Performance review created.";
            return RedirectToPage("Details", new { id = review.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadOptionsAsync();
            return Page();
        }
    }

    private async Task LoadOptionsAsync()
    {
        var (employees, _) = await employeeService.GetPagedAsync(1, 500, status: EmployeeStatus.Active);
        EmployeeOptions = employees.Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();
        ReviewerOptions = employees.Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();
    }
}
