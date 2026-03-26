using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Inspections;

public sealed class CreateModel(
    IInspectionService inspectionService,
    IUnitService unitService,
    ILeaseService leaseService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> Units { get; set; } = [];
    public List<SelectListItem> Leases { get; set; } = [];

    public sealed class InputModel
    {
        [Required]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required]
        [Display(Name = "Inspection Type")]
        public InspectionType InspectionType { get; set; }

        [Required]
        [Display(Name = "Scheduled Date")]
        public DateOnly ScheduledDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        [Required]
        [MaxLength(200)]
        [Display(Name = "Inspector Name")]
        public string InspectorName { get; set; } = string.Empty;

        [Display(Name = "Lease (optional)")]
        public int? LeaseId { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadSelectListsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
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

        await inspectionService.CreateAsync(inspection);

        TempData["SuccessMessage"] = "Inspection scheduled successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadSelectListsAsync()
    {
        var units = await unitService.GetUnitsAsync(null, null, null, null, null, null, 1, 500);
        Units = units.Items.Select(u => new SelectListItem(
            $"{u.Property.Name} - Unit #{u.UnitNumber}", u.Id.ToString())).ToList();

        var leases = await leaseService.GetActiveLeasesAsync();
        Leases = leases.Select(l => new SelectListItem(
            $"{l.Tenant.FullName} — {l.Unit.Property.Name} #{l.Unit.UnitNumber}", l.Id.ToString())).ToList();
    }
}
