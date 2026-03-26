using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class CancelModel : PageModel
{
    private readonly IEventService _eventService;

    public CancelModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    public Event Event { get; set; } = null!;
    public int RegistrationCount { get; set; }

    [BindProperty, Required(ErrorMessage = "Cancellation reason is required.")]
    public string CancellationReason { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var evt = await _eventService.GetEventByIdAsync(id);
        if (evt == null) return NotFound();
        Event = evt;
        RegistrationCount = evt.CurrentRegistrations + evt.WaitlistCount;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            var evt = await _eventService.GetEventByIdAsync(id);
            if (evt == null) return NotFound();
            Event = evt;
            RegistrationCount = evt.CurrentRegistrations + evt.WaitlistCount;
            return Page();
        }

        var result = await _eventService.CancelEventAsync(id, CancellationReason);
        if (!result)
        {
            TempData["ErrorMessage"] = "This event cannot be cancelled.";
            return RedirectToPage("Details", new { id });
        }

        TempData["SuccessMessage"] = "Event cancelled successfully.";
        return RedirectToPage("Details", new { id });
    }
}
