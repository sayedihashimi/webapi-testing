using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.CheckIn;

public class ProcessModel : PageModel
{
    private readonly IRegistrationService _registrationService;
    private readonly ICheckInService _checkInService;

    public ProcessModel(IRegistrationService registrationService, ICheckInService checkInService)
    {
        _registrationService = registrationService;
        _checkInService = checkInService;
    }

    public Registration? Registration { get; set; }
    public int EventId { get; set; }

    [BindProperty, Required(ErrorMessage = "Staff name is required."), MaxLength(200)]
    public string CheckedInBy { get; set; } = string.Empty;

    [BindProperty, MaxLength(500)]
    public string? Notes { get; set; }

    public async Task<IActionResult> OnGetAsync(int eventId, int registrationId)
    {
        EventId = eventId;
        Registration = await _registrationService.GetRegistrationByIdAsync(registrationId);
        if (Registration == null || Registration.EventId != eventId) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int eventId, int registrationId)
    {
        EventId = eventId;
        if (!ModelState.IsValid)
        {
            Registration = await _registrationService.GetRegistrationByIdAsync(registrationId);
            return Page();
        }

        var (success, error) = await _checkInService.ProcessCheckInAsync(registrationId, CheckedInBy, Notes);
        if (!success)
        {
            TempData["ErrorMessage"] = error;
            return RedirectToPage("Index", new { eventId });
        }

        TempData["SuccessMessage"] = "Check-in processed successfully!";
        return RedirectToPage("Index", new { eventId });
    }
}
