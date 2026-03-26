using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class CancelModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IRegistrationService _registrationService;

    public CancelModel(IEventService eventService, IRegistrationService registrationService)
    {
        _eventService = eventService;
        _registrationService = registrationService;
    }

    public Event Event { get; set; } = null!;
    public int ActiveRegistrationCount { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Please provide a reason for cancellation.")]
        [MaxLength(1000)]
        [Display(Name = "Cancellation Reason")]
        public string CancellationReason { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var evt = await _eventService.GetByIdWithDetailsAsync(id, ct);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;
        ActiveRegistrationCount = evt.Registrations
            .Count(r => r.Status != RegistrationStatus.Cancelled);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        var evt = await _eventService.GetByIdWithDetailsAsync(id, ct);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;
        ActiveRegistrationCount = evt.Registrations
            .Count(r => r.Status != RegistrationStatus.Cancelled);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var success = await _eventService.CancelAsync(id, Input.CancellationReason, ct);
        if (!success)
        {
            TempData["ErrorMessage"] = "Unable to cancel this event.";
            return RedirectToPage("Details", new { id });
        }

        TempData["SuccessMessage"] = "Event has been cancelled.";
        return RedirectToPage("Details", new { id });
    }
}
