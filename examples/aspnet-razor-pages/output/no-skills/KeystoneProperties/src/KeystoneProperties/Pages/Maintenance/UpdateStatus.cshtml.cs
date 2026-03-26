using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Maintenance;

public class UpdateStatusModel : PageModel
{
    private readonly IMaintenanceService _maintenanceService;
    public UpdateStatusModel(IMaintenanceService maintenanceService) { _maintenanceService = maintenanceService; }

    public new MaintenanceRequest Request { get; set; } = null!;
    public List<MaintenanceStatus> ValidTransitions{ get; set; } = new();
    [BindProperty] public int RequestId { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "New Status")]
        public MaintenanceStatus NewStatus { get; set; }
        [MaxLength(200), Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }
        [MaxLength(2000), Display(Name = "Completion Notes")]
        public string? CompletionNotes { get; set; }
        [Display(Name = "Estimated Cost"), DataType(DataType.Currency)]
        public decimal? EstimatedCost { get; set; }
        [Display(Name = "Actual Cost"), DataType(DataType.Currency)]
        public decimal? ActualCost { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var req = await _maintenanceService.GetWithDetailsAsync(id);
        if (req == null) return NotFound();
        Request = req;
        RequestId = id;
        ValidTransitions = await _maintenanceService.GetValidTransitionsAsync(req.Status);
        Input.AssignedTo = req.AssignedTo;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var req = await _maintenanceService.GetWithDetailsAsync(RequestId);
        if (req == null) return NotFound();
        Request = req;
        ValidTransitions = await _maintenanceService.GetValidTransitionsAsync(req.Status);

        if (!ModelState.IsValid) return Page();

        var (success, error) = await _maintenanceService.UpdateStatusAsync(
            RequestId, Input.NewStatus, Input.AssignedTo,
            Input.CompletionNotes, Input.EstimatedCost, Input.ActualCost);

        if (!success) { ModelState.AddModelError(string.Empty, error!); return Page(); }

        TempData["SuccessMessage"] = $"Maintenance request status updated to {Input.NewStatus}.";
        return RedirectToPage("Details", new { id = RequestId });
    }
}
