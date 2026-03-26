using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Reviews;

public class CreateModel : PageModel
{
    private readonly IReviewService _reviewService;
    private readonly IEmployeeService _employeeService;

    public CreateModel(IReviewService reviewService, IEmployeeService employeeService)
    {
        _reviewService = reviewService;
        _employeeService = employeeService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> EmployeeOptions { get; set; } = new();
    public List<SelectListItem> ReviewerOptions { get; set; } = new();

    public class InputModel
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int ReviewerId { get; set; }

        [Required]
        public DateOnly ReviewPeriodStart { get; set; } = new DateOnly(DateTime.UtcNow.Year, 1, 1);

        [Required]
        public DateOnly ReviewPeriodEnd { get; set; } = new DateOnly(DateTime.UtcNow.Year, 12, 31);
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

        if (Input.ReviewPeriodEnd <= Input.ReviewPeriodStart)
        {
            ModelState.AddModelError("Input.ReviewPeriodEnd", "End date must be after start date.");
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

            await _reviewService.CreateAsync(review);
            TempData["Success"] = "Performance review created.";
            return RedirectToPage("Details", new { id = review.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            await LoadOptionsAsync();
            return Page();
        }
    }

    private async Task LoadOptionsAsync()
    {
        var employees = await _employeeService.GetAllActiveAsync();
        EmployeeOptions = employees.Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();
        ReviewerOptions = employees.Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();
    }
}
