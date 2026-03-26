using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class CancelModel : PageModel
{
    private readonly IEventService _eventService;
    public CancelModel(IEventService eventService) => _eventService = eventService;

    public Event? Event { get; set; }

    [BindProperty, Required(ErrorMessage = "Cancellation reason is required.")]
    public string Reason { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Event = await _eventService.GetEventByIdAsync(id);
        if (Event == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            Event = await _eventService.GetEventByIdAsync(id);
            return Page();
        }
        var result = await _eventService.CancelEventAsync(id, Reason);
        if (!result)
        {
            TempData["ErrorMessage"] = "Cannot cancel this event.";
            return RedirectToPage("Details", new { id });
        }
        TempData["SuccessMessage"] = "Event cancelled successfully.";
        return RedirectToPage("Details", new { id });
    }
}
