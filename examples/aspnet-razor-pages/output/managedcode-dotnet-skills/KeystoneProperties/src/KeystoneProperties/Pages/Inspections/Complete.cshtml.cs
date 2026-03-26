using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Inspections;

public class CompleteModel(IInspectionService inspectionService) : PageModel
{
    public Inspection Inspection { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Overall Condition")]
        public OverallCondition OverallCondition { get; set; }

        [MaxLength(5000)]
        public string? Notes { get; set; }

        [Display(Name = "Follow-up Required")]
        public bool FollowUpRequired { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var inspection = await inspectionService.GetInspectionWithDetailsAsync(id);
        if (inspection is null)
        {
            return NotFound();
        }

        if (inspection.CompletedDate.HasValue)
        {
            return RedirectToPage("/Inspections/Details", new { id });
        }

        Inspection = inspection;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var inspection = await inspectionService.GetInspectionWithDetailsAsync(id);
        if (inspection is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            Inspection = inspection;
            return Page();
        }

        await inspectionService.CompleteInspectionAsync(id, Input.OverallCondition, Input.Notes, Input.FollowUpRequired);

        TempData["SuccessMessage"] = "Inspection completed successfully.";
        return RedirectToPage("/Inspections/Details", new { id });
    }
}
