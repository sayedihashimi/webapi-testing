using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Pages.Inspections;

public class CreateModel : PageModel
{
    private readonly IInspectionService _inspectionService;
    private readonly ApplicationDbContext _context;

    public CreateModel(IInspectionService inspectionService, ApplicationDbContext context)
    {
        _inspectionService = inspectionService;
        _context = context;
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    public List<Unit> UnitList { get; set; } = new();
    public List<Lease> LeaseList { get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "Unit")] public int UnitId { get; set; }
        [Required, Display(Name = "Inspection Type")] public InspectionType InspectionType { get; set; }
        [Required, Display(Name = "Scheduled Date")] public DateOnly ScheduledDate { get; set; }
        [Required, MaxLength(200), Display(Name = "Inspector Name")]
        public string InspectorName { get; set; } = string.Empty;
        [Display(Name = "Lease")] public int? LeaseId { get; set; }
        [MaxLength(5000)] public string? Notes { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadDropdowns();
        Input.ScheduledDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDropdowns();
        if (!ModelState.IsValid) return Page();

        var inspection = new Inspection
        {
            UnitId = Input.UnitId,
            InspectionType = Input.InspectionType,
            ScheduledDate = Input.ScheduledDate,
            InspectorName = Input.InspectorName,
            LeaseId = Input.LeaseId,
            Notes = Input.Notes
        };

        await _inspectionService.CreateAsync(inspection);
        TempData["SuccessMessage"] = "Inspection scheduled.";
        return RedirectToPage("Details", new { id = inspection.Id });
    }

    private async Task LoadDropdowns()
    {
        UnitList = await _context.Units.Include(u => u.Property).OrderBy(u => u.Property.Name).ThenBy(u => u.UnitNumber).ToListAsync();
        LeaseList = await _context.Leases.Include(l => l.Tenant).Where(l => l.Status == LeaseStatus.Active).ToListAsync();
    }
}
