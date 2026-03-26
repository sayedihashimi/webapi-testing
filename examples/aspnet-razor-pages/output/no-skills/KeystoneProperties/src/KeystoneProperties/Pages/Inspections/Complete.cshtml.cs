using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Inspections;

public class CompleteModel : PageModel
{
    private readonly IInspectionService _inspectionService;
    public CompleteModel(IInspectionService inspectionService) { _inspectionService = inspectionService; }

    public Inspection Inspection { get; set; } = null!;
    [BindProperty] public int InspectionId { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "Overall Condition")]
        public OverallCondition OverallCondition { get; set; }
        [MaxLength(5000)] public string? Notes { get; set; }
        [Display(Name = "Follow-Up Required")]
        public bool FollowUpRequired { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var inspection = await _inspectionService.GetWithDetailsAsync(id);
        if (inspection == null) return NotFound();
        Inspection = inspection;
        InspectionId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var inspection = await _inspectionService.GetWithDetailsAsync(InspectionId);
        if (inspection == null) return NotFound();
        Inspection = inspection;

        if (!ModelState.IsValid) return Page();

        var (success, error) = await _inspectionService.CompleteAsync(
            InspectionId, Input.OverallCondition, Input.Notes, Input.FollowUpRequired);

        if (!success) { ModelState.AddModelError(string.Empty, error!); return Page(); }

        TempData["SuccessMessage"] = "Inspection completed.";
        return RedirectToPage("Details", new { id = InspectionId });
    }
}
