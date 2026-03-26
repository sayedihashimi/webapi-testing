using System.ComponentModel.DataAnnotations;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Reviews;

public sealed class CreateModel(IReviewService reviewService, IEmployeeService employeeService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> EmployeeList { get; set; } = [];
    public List<SelectListItem> ReviewerList { get; set; } = [];

    public sealed class InputModel
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int ReviewerId { get; set; }

        [Required]
        public DateOnly ReviewPeriodStart { get; set; }

        [Required]
        public DateOnly ReviewPeriodEnd { get; set; }
    }

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadDropdownsAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(ct);
            return Page();
        }

        try
        {
            var review = await reviewService.CreateAsync(Input.EmployeeId, Input.ReviewerId, Input.ReviewPeriodStart, Input.ReviewPeriodEnd, ct);
            TempData["SuccessMessage"] = "Performance review created.";
            return RedirectToPage("Details", new { id = review.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadDropdownsAsync(ct);
            return Page();
        }
    }

    private async Task LoadDropdownsAsync(CancellationToken ct)
    {
        var employees = await employeeService.GetAllAsync(null, null, null, EmployeeStatus.Active, 1, 1000, ct);
        EmployeeList = employees.Items.Select(e => new SelectListItem($"{e.FirstName} {e.LastName}", e.Id.ToString())).ToList();
        ReviewerList = employees.Items.Select(e => new SelectListItem($"{e.FirstName} {e.LastName}", e.Id.ToString())).ToList();
    }
}
