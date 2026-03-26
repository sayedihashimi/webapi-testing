using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Inspections;

public sealed class CompleteModel(IInspectionService inspectionService) : PageModel
{
    public Inspection Inspection { get; set; } = null!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Please enter the completed date.")]
        [Display(Name = "Completed Date")]
        public DateOnly CompletedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "Please select the overall condition.")]
        [Display(Name = "Overall Condition")]
        public OverallCondition OverallCondition { get; set; }

        [MaxLength(5000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Follow-Up Required")]
        public bool FollowUpRequired { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var inspection = await inspectionService.GetByIdAsync(id, ct);
        if (inspection is null)
        {
            return NotFound();
        }

        if (inspection.CompletedDate.HasValue)
        {
            TempData["ErrorMessage"] = "This inspection has already been completed.";
            return RedirectToPage("./Details", new { id });
        }

        Inspection = inspection;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        var inspection = await inspectionService.GetByIdAsync(id, ct);
        if (inspection is null)
        {
            return NotFound();
        }

        if (inspection.CompletedDate.HasValue)
        {
            TempData["ErrorMessage"] = "This inspection has already been completed.";
            return RedirectToPage("./Details", new { id });
        }

        if (!ModelState.IsValid)
        {
            Inspection = inspection;
            return Page();
        }

        var (success, error) = await inspectionService.CompleteAsync(
            id,
            Input.CompletedDate,
            Input.OverallCondition,
            Input.Notes,
            Input.FollowUpRequired,
            ct);

        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to complete inspection.";
            Inspection = inspection;
            return Page();
        }

        TempData["SuccessMessage"] = "Inspection completed successfully.";
        return RedirectToPage("./Details", new { id });
    }
}
