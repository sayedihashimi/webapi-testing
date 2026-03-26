using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Maintenance;

public class UpdateStatusModel(IMaintenanceService maintenanceService) : PageModel
{
    public new MaintenanceRequest Request { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<MaintenanceStatus> ValidNextStatuses { get; set; } = [];

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
        public decimal? EstimatedCost { get; set; }

        [Display(Name = "Actual Cost")]
        public decimal? ActualCost { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var request = await maintenanceService.GetRequestWithDetailsAsync(id);
        if (request is null)
        {
            return NotFound();
        }

        if (request.Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
        {
            return RedirectToPage("/Maintenance/Details", new { id });
        }

        Request = request;
        ValidNextStatuses = GetValidNextStatuses(request.Status);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var request = await maintenanceService.GetRequestWithDetailsAsync(id);
        if (request is null)
        {
            return NotFound();
        }

        if (Input.NewStatus == MaintenanceStatus.Assigned && string.IsNullOrWhiteSpace(Input.AssignedTo))
        {
            ModelState.AddModelError("Input.AssignedTo", "Assigned To is required when assigning a request.");
        }

        if (!ModelState.IsValid)
        {
            Request = request;
            ValidNextStatuses = GetValidNextStatuses(request.Status);
            return Page();
        }

        var (success, errorMessage) = await maintenanceService.UpdateStatusAsync(
            id, Input.NewStatus, Input.AssignedTo, Input.CompletionNotes,
            Input.EstimatedCost, Input.ActualCost);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToPage("/Maintenance/Details", new { id });
        }

        TempData["SuccessMessage"] = "Maintenance request status updated successfully.";
        return RedirectToPage("/Maintenance/Details", new { id });
    }

    private static List<MaintenanceStatus> GetValidNextStatuses(MaintenanceStatus currentStatus)
    {
        return currentStatus switch
        {
            MaintenanceStatus.Submitted => [MaintenanceStatus.Assigned, MaintenanceStatus.Cancelled],
            MaintenanceStatus.Assigned => [MaintenanceStatus.InProgress, MaintenanceStatus.Cancelled],
            MaintenanceStatus.InProgress => [MaintenanceStatus.Completed, MaintenanceStatus.Cancelled],
            _ => []
        };
    }
}
