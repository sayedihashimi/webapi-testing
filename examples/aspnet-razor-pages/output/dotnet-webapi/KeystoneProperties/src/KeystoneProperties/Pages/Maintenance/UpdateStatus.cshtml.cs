using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Maintenance;

public sealed class UpdateStatusModel(IMaintenanceService maintenanceService) : PageModel
{
    public new MaintenanceRequest Request { get; set; } = default!;

    public List<MaintenanceStatus> ValidNextStatuses { get; set; } = [];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required(ErrorMessage = "New status is required.")]
        [Display(Name = "New Status")]
        public MaintenanceStatus NewStatus { get; set; }

        [MaxLength(200)]
        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Completion Notes")]
        public string? CompletionNotes { get; set; }

        [Display(Name = "Estimated Cost")]
        [Range(0, double.MaxValue, ErrorMessage = "Estimated cost must be non-negative.")]
        public decimal? EstimatedCost { get; set; }

        [Display(Name = "Actual Cost")]
        [Range(0, double.MaxValue, ErrorMessage = "Actual cost must be non-negative.")]
        public decimal? ActualCost { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var request = await maintenanceService.GetByIdAsync(id, ct);
        if (request is null)
        {
            return NotFound();
        }

        Request = request;
        ValidNextStatuses = GetValidTransitions(request.Status);

        if (ValidNextStatuses.Count == 0)
        {
            TempData["ErrorMessage"] = "This request is in a terminal state and cannot be updated.";
            return RedirectToPage("Details", new { id });
        }

        Input.EstimatedCost = request.EstimatedCost;
        Input.ActualCost = request.ActualCost;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        var request = await maintenanceService.GetByIdAsync(id, ct);
        if (request is null)
        {
            return NotFound();
        }

        Request = request;
        ValidNextStatuses = GetValidTransitions(request.Status);

        if (!ValidNextStatuses.Contains(Input.NewStatus))
        {
            ModelState.AddModelError("Input.NewStatus", "Invalid status transition.");
        }

        if (Input.NewStatus == MaintenanceStatus.Assigned && string.IsNullOrWhiteSpace(Input.AssignedTo))
        {
            ModelState.AddModelError("Input.AssignedTo", "Assigned To is required when transitioning to Assigned.");
        }

        if (Input.NewStatus == MaintenanceStatus.Completed && string.IsNullOrWhiteSpace(Input.CompletionNotes))
        {
            ModelState.AddModelError("Input.CompletionNotes", "Completion notes are required when completing a request.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var (success, error) = await maintenanceService.UpdateStatusAsync(
            id,
            Input.NewStatus,
            Input.AssignedTo,
            Input.CompletionNotes,
            Input.EstimatedCost,
            Input.ActualCost,
            ct);

        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to update status.";
            return Page();
        }

        TempData["SuccessMessage"] = $"Request status updated to {Input.NewStatus}.";
        return RedirectToPage("Details", new { id });
    }

    private static List<MaintenanceStatus> GetValidTransitions(MaintenanceStatus current) => current switch
    {
        MaintenanceStatus.Submitted => [MaintenanceStatus.Assigned, MaintenanceStatus.Cancelled],
        MaintenanceStatus.Assigned => [MaintenanceStatus.InProgress, MaintenanceStatus.Cancelled],
        MaintenanceStatus.InProgress => [MaintenanceStatus.Completed, MaintenanceStatus.Cancelled],
        _ => []
    };
}
