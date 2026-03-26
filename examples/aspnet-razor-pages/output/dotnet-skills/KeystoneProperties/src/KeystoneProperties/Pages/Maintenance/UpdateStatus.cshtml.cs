using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Pages.Maintenance;

public class UpdateStatusModel : PageModel
{
    private readonly IMaintenanceService _maintenanceService;

    public UpdateStatusModel(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    public new MaintenanceRequest Request { get; set; } = default!;
    public SelectList ValidStatuses { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "New Status")]
        public MaintenanceStatus NewStatus { get; set; }

        [MaxLength(200)]
        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Completion Notes")]
        public string? CompletionNotes { get; set; }

        [Display(Name = "Estimated Cost")]
        [DataType(DataType.Currency)]
        public decimal? EstimatedCost { get; set; }

        [Display(Name = "Actual Cost")]
        [DataType(DataType.Currency)]
        public decimal? ActualCost { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var request = await _maintenanceService.GetWithDetailsAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        Request = request;
        var validStatuses = await _maintenanceService.GetValidNextStatuses(request.Status);
        if (validStatuses.Count == 0)
        {
            TempData["ErrorMessage"] = "No status transitions available for this request.";
            return RedirectToPage("Details", new { id });
        }

        ValidStatuses = new SelectList(
            validStatuses.Select(s => new { Value = (int)s, Text = s.ToString() }),
            "Value", "Text");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var request = await _maintenanceService.GetWithDetailsAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            Request = request;
            var statuses = await _maintenanceService.GetValidNextStatuses(request.Status);
            ValidStatuses = new SelectList(
                statuses.Select(s => new { Value = (int)s, Text = s.ToString() }),
                "Value", "Text");
            return Page();
        }

        var (success, error) = await _maintenanceService.UpdateStatusAsync(
            id, Input.NewStatus, Input.AssignedTo, Input.CompletionNotes,
            Input.EstimatedCost, Input.ActualCost);

        if (!success)
        {
            TempData["ErrorMessage"] = error;
            return RedirectToPage("UpdateStatus", new { id });
        }

        TempData["SuccessMessage"] = $"Status updated to {Input.NewStatus}.";
        return RedirectToPage("Details", new { id });
    }
}
