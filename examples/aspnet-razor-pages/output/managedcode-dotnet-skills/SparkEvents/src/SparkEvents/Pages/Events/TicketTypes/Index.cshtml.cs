using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events.TicketTypes;

public class IndexModel(IEventService eventService, ITicketTypeService ticketTypeService) : PageModel
{
    public Event Event { get; set; } = null!;
    public IReadOnlyList<TicketType> TicketTypes { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var ev = await eventService.GetByIdAsync(eventId);
        if (ev is null)
        {
            return NotFound();
        }

        Event = ev;
        TicketTypes = await ticketTypeService.GetByEventIdAsync(eventId);
        return Page();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int eventId, int ticketTypeId)
    {
        await ticketTypeService.DeactivateAsync(ticketTypeId);
        TempData["SuccessMessage"] = "Ticket type deactivated.";
        return RedirectToPage("Index", new { eventId });
    }
}
