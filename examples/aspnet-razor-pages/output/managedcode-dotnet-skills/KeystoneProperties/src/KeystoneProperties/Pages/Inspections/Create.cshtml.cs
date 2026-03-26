using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Inspections;

public class CreateModel(IInspectionService inspectionService, IUnitService unitService, ILeaseService leaseService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<Unit> Units { get; set; } = [];

    public List<Lease> Leases { get; set; } = [];

    public class InputModel
    {
        [Required]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required]
        [Display(Name = "Inspection Type")]
        public InspectionType InspectionType { get; set; }

        [Required]
        [Display(Name = "Scheduled Date")]
        public DateOnly ScheduledDate { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Inspector Name")]
        public string InspectorName { get; set; } = string.Empty;

        [Display(Name = "Related Lease")]
        public int? LeaseId { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        var inspection = new Inspection
        {
            UnitId = Input.UnitId,
            InspectionType = Input.InspectionType,
            ScheduledDate = Input.ScheduledDate,
            InspectorName = Input.InspectorName,
            LeaseId = Input.LeaseId
        };

        await inspectionService.CreateInspectionAsync(inspection);

        TempData["SuccessMessage"] = "Inspection scheduled successfully.";
        return RedirectToPage("/Inspections/Index");
    }

    private async Task LoadDropdownsAsync()
    {
        var unitResult = await unitService.GetUnitsAsync(null, null, null, null, null, null, 1, 1000);
        Units = [.. unitResult];
        var leaseResult = await leaseService.GetLeasesAsync(LeaseStatus.Active, null, 1, 1000);
        Leases = [.. leaseResult];
    }
}
