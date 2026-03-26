using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Maintenance;

public sealed class CreateModel(
    IMaintenanceService maintenanceService,
    IUnitService unitService,
    ITenantService tenantService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<Unit> Units { get; set; } = [];

    public List<Tenant> Tenants { get; set; } = [];

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Unit is required.")]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required(ErrorMessage = "Tenant is required.")]
        [Display(Name = "Tenant")]
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
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

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadDropdownsAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (Input.Priority == MaintenancePriority.Emergency && string.IsNullOrWhiteSpace(Input.AssignedTo))
        {
            ModelState.AddModelError("Input.AssignedTo", "Assigned To is required for Emergency priority requests.");
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(ct);
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
            AssignedTo = Input.AssignedTo,
            Status = MaintenanceStatus.Submitted,
            SubmittedDate = DateTime.UtcNow
        };

        var created = await maintenanceService.CreateAsync(request, ct);
        TempData["SuccessMessage"] = $"Maintenance request \"{created.Title}\" created successfully.";
        return RedirectToPage("Details", new { id = created.Id });
    }

    private async Task LoadDropdownsAsync(CancellationToken ct)
    {
        var unitsResult = await unitService.GetAllAsync(null, null, null, null, null, null, 1, 1000, ct);
        Units = unitsResult.Items.ToList();
        Tenants = await tenantService.GetActiveTenantsAsync(ct);
    }
}
