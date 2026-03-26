using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Pages.Maintenance;

public class CreateModel : PageModel
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public CreateModel(
        IMaintenanceService maintenanceService,
        ApplicationDbContext context,
        ITenantService tenantService)
    {
        _maintenanceService = maintenanceService;
        _context = context;
        _tenantService = tenantService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList UnitList { get; set; } = default!;
    public SelectList TenantList { get; set; } = default!;

    public class InputModel
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
            AssignedTo = Input.AssignedTo,
            Status = MaintenanceStatus.Submitted,
            SubmittedDate = DateTime.UtcNow
        };

        await _maintenanceService.CreateAsync(request);
        TempData["SuccessMessage"] = "Maintenance request created successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadDropdownsAsync()
    {
        var units = await _context.Units
            .Include(u => u.Property)
            .OrderBy(u => u.Property.Name)
            .ThenBy(u => u.UnitNumber)
            .ToListAsync();

        UnitList = new SelectList(
            units.Select(u => new { u.Id, Display = $"{u.Property.Name} - Unit {u.UnitNumber}" }),
            "Id", "Display");

        var tenants = await _tenantService.GetAllActiveAsync();
        TenantList = new SelectList(tenants, "Id", "FullName");
    }
}
