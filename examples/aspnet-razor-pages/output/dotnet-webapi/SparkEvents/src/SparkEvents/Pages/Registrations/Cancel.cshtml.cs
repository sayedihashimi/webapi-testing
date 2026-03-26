using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Registrations;

public sealed class CancelModel : PageModel
{
    private readonly IRegistrationService _registrationService;

    public CancelModel(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public Registration Registration { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        public int Id { get; set; }

        [MaxLength(500)]
        [Display(Name = "Cancellation Reason")]
        public string? CancellationReason { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var registration = await _registrationService.GetByIdWithDetailsAsync(id);
        if (registration is null)
        {
            return NotFound();
        }

        if (registration.Status == RegistrationStatus.Cancelled)
        {
            TempData["ErrorMessage"] = "This registration has already been cancelled.";
            return RedirectToPage("Details", new { id });
        }

        Registration = registration;
        Input = new InputModel { Id = id };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var registration = await _registrationService.GetByIdWithDetailsAsync(Input.Id);
        if (registration is null)
        {
            return NotFound();
        }

        Registration = registration;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _registrationService.CancelRegistrationAsync(Input.Id, Input.CancellationReason);
            TempData["SuccessMessage"] = $"Registration {registration.ConfirmationNumber} has been cancelled.";
            return RedirectToPage("Details", new { id = Input.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("Details", new { id = Input.Id });
        }
    }
}
