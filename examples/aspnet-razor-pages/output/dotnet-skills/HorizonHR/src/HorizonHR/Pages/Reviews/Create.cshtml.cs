using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

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

    public List<Employee> Employees { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required]
        [Display(Name = "Reviewer")]
        public int ReviewerId { get; set; }

        [Required]
        [Display(Name = "Review Period Start")]
        public DateOnly ReviewPeriodStart { get; set; }

        [Required]
        [Display(Name = "Review Period End")]
        public DateOnly ReviewPeriodEnd { get; set; }
    }

    public async Task OnGetAsync(int? employeeId)
    {
        await LoadEmployeesAsync();

        if (employeeId.HasValue)
        {
            Input.EmployeeId = employeeId.Value;
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId.Value);
            if (employee?.ManagerId != null)
            {
                Input.ReviewerId = employee.ManagerId.Value;
            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadEmployeesAsync();
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

            var created = await _reviewService.CreateReviewAsync(review);
            TempData["SuccessMessage"] = "Performance review created successfully.";
            return RedirectToPage("Details", new { id = created.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadEmployeesAsync();
            return Page();
        }
    }

    private async Task LoadEmployeesAsync()
    {
        var (employees, _) = await _employeeService.GetEmployeesAsync(null, null, null, null, 1, int.MaxValue);
        Employees = employees;
    }
}
