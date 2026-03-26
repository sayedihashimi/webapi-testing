using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList UnitOptions { get; set; } = null!;
    public SelectList LeaseOptions { get; set; } = null!;

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
        public DateOnly ScheduledDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        [MaxLength(200)]
        [Display(Name = "Inspector Name")]
        public string InspectorName { get; set; } = string.Empty;

        [Display(Name = "Lease")]
        public int? LeaseId { get; set; }

        [MaxLength(5000)]
        public string? Notes { get; set; }
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
            LeaseId = Input.LeaseId,
            Notes = Input.Notes
        };

        await _inspectionService.CreateAsync(inspection);
        TempData["SuccessMessage"] = "Inspection scheduled successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadDropdownsAsync()
    {
        var units = await _context.Units
            .Include(u => u.Property)
            .OrderBy(u => u.Property.Name)
            .ThenBy(u => u.UnitNumber)
            .Select(u => new { u.Id, Display = u.Property.Name + " - " + u.UnitNumber })
            .AsNoTracking()
            .ToListAsync();

        UnitOptions = new SelectList(units, "Id", "Display");

        var leases = await _context.Leases
            .Include(l => l.Tenant)
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .OrderByDescending(l => l.StartDate)
            .Select(l => new { l.Id, Display = l.Tenant.FullName + " (" + l.Unit.Property.Name + " - " + l.Unit.UnitNumber + ")" })
            .AsNoTracking()
            .ToListAsync();

        LeaseOptions = new SelectList(leases, "Id", "Display");
    }
}
