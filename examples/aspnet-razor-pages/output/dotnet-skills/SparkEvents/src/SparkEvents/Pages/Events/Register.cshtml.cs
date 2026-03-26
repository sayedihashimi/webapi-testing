using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class RegisterModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IAttendeeService _attendeeService;
    private readonly ITicketTypeService _ticketTypeService;
    private readonly IRegistrationService _registrationService;

    public RegisterModel(IEventService eventService, IAttendeeService attendeeService,
        ITicketTypeService ticketTypeService, IRegistrationService registrationService)
    {
        _eventService = eventService;
        _attendeeService = attendeeService;
        _ticketTypeService = ticketTypeService;
        _registrationService = registrationService;
    }

    public Event? Event { get; set; }
    public List<TicketType> TicketTypes { get; set; } = new();
    public SelectList AttendeeList { get; set; } = null!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "Attendee")]
        public int AttendeeId { get; set; }
        [Required, Display(Name = "Ticket Type")]
        public int TicketTypeId { get; set; }
        [MaxLength(1000), Display(Name = "Special Requests")]
        public string? SpecialRequests { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        Event = await _eventService.GetEventByIdAsync(eventId);
        if (Event == null) return NotFound();
        await PopulateAsync(eventId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int eventId)
    {
        Event = await _eventService.GetEventByIdAsync(eventId);
        if (Event == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateAsync(eventId);
            return Page();
        }

        var (registration, error) = await _registrationService.RegisterAsync(
            eventId, Input.AttendeeId, Input.TicketTypeId, Input.SpecialRequests);

        if (registration == null)
        {
            TempData["ErrorMessage"] = error;
            await PopulateAsync(eventId);
            return Page();
        }

        TempData["SuccessMessage"] = $"Registration successful! Confirmation: {registration.ConfirmationNumber}. Status: {registration.Status}.";
        return RedirectToPage("/Registrations/Details", new { id = registration.Id });
    }

    private async Task PopulateAsync(int eventId)
    {
        TicketTypes = await _ticketTypeService.GetTicketTypesForEventAsync(eventId);
        var attendees = await _attendeeService.GetAllAttendeesAsync();
        AttendeeList = new SelectList(attendees.Select(a => new { a.Id, Display = $"{a.FullName} ({a.Email})" }), "Id", "Display");
    }
}
