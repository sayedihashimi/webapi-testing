using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Registrations;

public class CancelModel : PageModel
{
    private readonly IRegistrationService _registrationService;

    public CancelModel(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public Registration Registration { get; set; } = null!;
    public bool CanCancel { get; set; }

    [BindProperty]
    public string? CancellationReason { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var reg = await _registrationService.GetRegistrationByIdAsync(id);
        if (reg == null) return NotFound();
        Registration = reg;
        CanCancel = (reg.Event.StartDate - DateTime.UtcNow).TotalHours >= 24
            && reg.Status == RegistrationStatus.Confirmed;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            var result = await _registrationService.CancelRegistrationAsync(id, CancellationReason);
            if (!result)
            {
                TempData["ErrorMessage"] = "Unable to cancel this registration.";
                return RedirectToPage("Details", new { id });
            }

            TempData["SuccessMessage"] = "Registration cancelled successfully.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("Details", new { id });
        }
    }
}
