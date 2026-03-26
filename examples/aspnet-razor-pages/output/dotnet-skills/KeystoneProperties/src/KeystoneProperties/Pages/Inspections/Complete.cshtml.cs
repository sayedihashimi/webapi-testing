using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Inspections;

public class CompleteModel : PageModel
{
    private readonly IInspectionService _inspectionService;

    public CompleteModel(IInspectionService inspectionService)
    {
        _inspectionService = inspectionService;
    }

    public Inspection Inspection { get; set; } = null!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Overall Condition")]
        public OverallCondition OverallCondition { get; set; }

        [MaxLength(5000)]
        public string? Notes { get; set; }

        [Display(Name = "Follow-Up Required")]
        public bool FollowUpRequired { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var inspection = await _inspectionService.GetWithDetailsAsync(id);
        if (inspection is null)
        {
            return NotFound();
        }

        if (inspection.CompletedDate.HasValue)
        {
            TempData["ErrorMessage"] = "This inspection has already been completed.";
            return RedirectToPage("Details", new { id });
        }

        Inspection = inspection;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var inspection = await _inspectionService.GetWithDetailsAsync(id);
        if (inspection is null)
        {
            return NotFound();
        }

        if (inspection.CompletedDate.HasValue)
        {
            TempData["ErrorMessage"] = "This inspection has already been completed.";
            return RedirectToPage("Details", new { id });
        }

        if (!ModelState.IsValid)
        {
            Inspection = inspection;
            return Page();
        }

        var (success, error) = await _inspectionService.CompleteAsync(
            id, Input.OverallCondition, Input.Notes, Input.FollowUpRequired);

        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to complete the inspection.";
            Inspection = inspection;
            return Page();
        }

        TempData["SuccessMessage"] = "Inspection completed successfully.";
        return RedirectToPage("Details", new { id });
    }
}
