using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Inspections;

public sealed class CreateModel(
    IInspectionService inspectionService,
    IUnitService unitService,
    ILeaseService leaseService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IReadOnlyList<Unit> Units { get; set; } = [];
    public IReadOnlyList<Lease> ActiveLeases { get; set; } = [];

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Please select a unit.")]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required(ErrorMessage = "Please select an inspection type.")]
        [Display(Name = "Inspection Type")]
        public InspectionType InspectionType { get; set; }

        [Required(ErrorMessage = "Please enter a scheduled date.")]
        [Display(Name = "Scheduled Date")]
        public DateOnly ScheduledDate { get; set; }

        [Required(ErrorMessage = "Please enter the inspector name.")]
        [MaxLength(200)]
        [Display(Name = "Inspector Name")]
        public string InspectorName { get; set; } = string.Empty;

        [Display(Name = "Linked Lease")]
        public int? LeaseId { get; set; }

        [MaxLength(5000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
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

        var inspection = new Inspection
        {
            UnitId = Input.UnitId,
            InspectionType = Input.InspectionType,
            ScheduledDate = Input.ScheduledDate,
            InspectorName = Input.InspectorName,
            LeaseId = Input.LeaseId,
            Notes = Input.Notes
        };

        await inspectionService.CreateAsync(inspection, ct);

        TempData["SuccessMessage"] = "Inspection scheduled successfully.";
        return RedirectToPage("./Index");
    }

    private async Task LoadDropdownsAsync(CancellationToken ct)
    {
        var unitsResult = await unitService.GetAllAsync(null, null, null, null, null, null, 1, 1000, ct);
        Units = unitsResult.Items;
        ActiveLeases = await leaseService.GetActiveLeasesAsync(ct);
    }
}
