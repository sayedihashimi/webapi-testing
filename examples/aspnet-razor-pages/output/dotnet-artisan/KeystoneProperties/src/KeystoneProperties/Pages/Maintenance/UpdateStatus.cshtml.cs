using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Maintenance;

public sealed class UpdateStatusModel(IMaintenanceService maintenanceService) : PageModel
{
    public new MaintenanceRequest Request { get; set; } = null!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ValidStatuses { get; set; } = null!;

    public sealed class InputModel
    {
        public int Id { get; set; }

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
        var request = await maintenanceService.GetByIdAsync(id);

        if (request is null)
        {
            return NotFound();
        }

        if (request.Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
        {
            return RedirectToPage("Details", new { id });
        }

        Request = request;
        Input.Id = id;
        Input.EstimatedCost = request.EstimatedCost;
        Input.ActualCost = request.ActualCost;
        LoadValidStatuses(request.Status);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var request = await maintenanceService.GetByIdAsync(Input.Id);

        if (request is null)
        {
            return NotFound();
        }

        if (Input.NewStatus == MaintenanceStatus.Assigned && string.IsNullOrWhiteSpace(Input.AssignedTo))
        {
            ModelState.AddModelError("Input.AssignedTo", "Assigned To is required when setting status to Assigned.");
        }

        if (Input.NewStatus == MaintenanceStatus.Completed && string.IsNullOrWhiteSpace(Input.CompletionNotes))
        {
            ModelState.AddModelError("Input.CompletionNotes", "Completion Notes are required when completing a request.");
        }

        if (!ModelState.IsValid)
        {
            Request = request;
            LoadValidStatuses(request.Status);
            return Page();
        }

        var (success, error) = await maintenanceService.UpdateStatusAsync(
            Input.Id, Input.NewStatus, Input.AssignedTo, Input.CompletionNotes, Input.EstimatedCost, Input.ActualCost);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update status.");
            Request = request;
            LoadValidStatuses(request.Status);
            return Page();
        }

        TempData["SuccessMessage"] = $"Status updated to {Input.NewStatus} successfully.";
        return RedirectToPage("Details", new { id = Input.Id });
    }

    private void LoadValidStatuses(MaintenanceStatus currentStatus)
    {
        var validTransitions = GetValidTransitions(currentStatus);
        ValidStatuses = new SelectList(
            validTransitions.Select(s => new { Value = (int)s, Text = s.ToString() }),
            "Value",
            "Text");
    }

    private static List<MaintenanceStatus> GetValidTransitions(MaintenanceStatus current) =>
        current switch
        {
            MaintenanceStatus.Submitted => [MaintenanceStatus.Assigned, MaintenanceStatus.Cancelled],
            MaintenanceStatus.Assigned => [MaintenanceStatus.InProgress, MaintenanceStatus.Cancelled],
            MaintenanceStatus.InProgress => [MaintenanceStatus.Completed, MaintenanceStatus.Cancelled],
            _ => []
        };
}
