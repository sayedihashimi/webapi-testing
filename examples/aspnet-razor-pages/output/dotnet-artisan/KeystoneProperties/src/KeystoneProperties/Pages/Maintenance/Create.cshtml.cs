using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Maintenance;

public sealed class CreateModel(
    IMaintenanceService maintenanceService,
    IUnitService unitService,
    ITenantService tenantService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList UnitList { get; set; } = null!;
    public SelectList TenantList { get; set; } = null!;

    public sealed class InputModel
    {
        [Required]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required]
        [Display(Name = "Tenant")]
        public int TenantId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        public MaintenancePriority Priority { get; set; }

        public MaintenanceCategory Category { get; set; }

        [MaxLength(200)]
        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.Priority == MaintenancePriority.Emergency && string.IsNullOrWhiteSpace(Input.AssignedTo))
        {
            ModelState.AddModelError("Input.AssignedTo", "Assigned To is required for Emergency priority requests.");
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        var request = new MaintenanceRequest
        {
            UnitId = Input.UnitId,
            TenantId = Input.TenantId,
            Title = Input.Title,
            Description = Input.Description,
            Priority = Input.Priority,
            Category = Input.Category,
            Status = MaintenanceStatus.Submitted,
            SubmittedDate = DateTime.UtcNow,
            AssignedTo = Input.Priority == MaintenancePriority.Emergency ? Input.AssignedTo : null
        };

        await maintenanceService.CreateAsync(request);

        TempData["SuccessMessage"] = "Maintenance request created successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadDropdownsAsync()
    {
        var units = await unitService.GetUnitsAsync(null, null, null, null, null, null, 1, int.MaxValue);
        UnitList = new SelectList(
            units.Items.Select(u => new { u.Id, Display = $"{u.Property.Name} - Unit #{u.UnitNumber}" }),
            "Id",
            "Display");

        var tenants = await tenantService.GetActiveTenantsAsync();
        TenantList = new SelectList(
            tenants.Select(t => new { t.Id, Display = t.FullName }),
            "Id",
            "Display");
    }
}
