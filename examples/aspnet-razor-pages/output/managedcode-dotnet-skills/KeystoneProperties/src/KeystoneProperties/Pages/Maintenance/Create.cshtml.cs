using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Maintenance;

public class CreateModel(IMaintenanceService maintenanceService, IUnitService unitService, ITenantService tenantService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<Unit> Units { get; set; } = [];

    public List<Tenant> Tenants { get; set; } = [];

    public class InputModel
    {
        [Required]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required]
        [Display(Name = "Tenant")]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public MaintenancePriority Priority { get; set; }

        [Required]
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
            SubmittedDate = DateTime.UtcNow
        };

        if (Input.Priority == MaintenancePriority.Emergency)
        {
            request.AssignedTo = Input.AssignedTo;
            request.AssignedDate = DateTime.UtcNow;
            request.Status = MaintenanceStatus.Assigned;
        }

        await maintenanceService.CreateRequestAsync(request);

        TempData["SuccessMessage"] = "Maintenance request created successfully.";
        return RedirectToPage("/Maintenance/Index");
    }

    private async Task LoadDropdownsAsync()
    {
        var unitResult = await unitService.GetUnitsAsync(null, null, null, null, null, null, 1, 1000);
        Units = [.. unitResult];
        Tenants = await tenantService.GetActiveTenantsAsync();
    }
}
