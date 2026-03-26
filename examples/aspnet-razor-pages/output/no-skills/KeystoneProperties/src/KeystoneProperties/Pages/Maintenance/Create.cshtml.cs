using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Pages.Maintenance;

public class CreateModel : PageModel
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly ApplicationDbContext _context;

    public CreateModel(IMaintenanceService maintenanceService, ApplicationDbContext context)
    {
        _maintenanceService = maintenanceService;
        _context = context;
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    public List<Unit> UnitList { get; set; } = new();
    public List<Tenant> TenantList { get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "Unit")] public int UnitId { get; set; }
        [Required, Display(Name = "Tenant")] public int TenantId { get; set; }
        [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
        [Required, MaxLength(2000)] public string Description { get; set; } = string.Empty;
        [Required] public MaintenancePriority Priority { get; set; }
        [Required] public MaintenanceCategory Category { get; set; }
        [MaxLength(200), Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }
        [Display(Name = "Estimated Cost"), DataType(DataType.Currency)]
        public decimal? EstimatedCost { get; set; }
    }

    public async Task OnGetAsync() { await LoadDropdowns(); }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDropdowns();
        if (!ModelState.IsValid) return Page();

        var request = new MaintenanceRequest
        {
            UnitId = Input.UnitId, TenantId = Input.TenantId,
            Title = Input.Title, Description = Input.Description,
            Priority = Input.Priority, Category = Input.Category,
            AssignedTo = Input.AssignedTo, EstimatedCost = Input.EstimatedCost
        };

        var (success, error) = await _maintenanceService.CreateAsync(request);
        if (!success) { ModelState.AddModelError(string.Empty, error!); return Page(); }

        TempData["SuccessMessage"] = "Maintenance request submitted.";
        return RedirectToPage("Details", new { id = request.Id });
    }

    private async Task LoadDropdowns()
    {
        UnitList = await _context.Units.Include(u => u.Property).OrderBy(u => u.Property.Name).ThenBy(u => u.UnitNumber).ToListAsync();
        TenantList = await _context.Tenants.Where(t => t.IsActive).OrderBy(t => t.LastName).ToListAsync();
    }
}
