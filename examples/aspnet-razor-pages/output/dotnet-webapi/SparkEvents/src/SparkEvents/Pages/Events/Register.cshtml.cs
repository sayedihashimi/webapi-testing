using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class RegisterModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IAttendeeService _attendeeService;
    private readonly ITicketTypeService _ticketTypeService;
    private readonly IRegistrationService _registrationService;

    public RegisterModel(
        IEventService eventService,
        IAttendeeService attendeeService,
        ITicketTypeService ticketTypeService,
        IRegistrationService registrationService)
    {
        _eventService = eventService;
        _attendeeService = attendeeService;
        _ticketTypeService = ticketTypeService;
        _registrationService = registrationService;
    }

    public Event Event { get; set; } = default!;
    public List<Attendee> Attendees { get; set; } = [];
    public List<TicketType> TicketTypes { get; set; } = [];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required]
        [Display(Name = "Attendee")]
        public int AttendeeId { get; set; }

        [Required]
        [Display(Name = "Ticket Type")]
        public int TicketTypeId { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Special Requests")]
        public string? SpecialRequests { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var evt = await _eventService.GetByIdAsync(eventId);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;
        await LoadFormDataAsync(eventId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int eventId)
    {
        var evt = await _eventService.GetByIdAsync(eventId);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;

        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(eventId);
            return Page();
        }

        try
        {
            var registration = await _registrationService.RegisterAsync(
                eventId,
                Input.AttendeeId,
                Input.TicketTypeId,
                Input.SpecialRequests);

            var statusMessage = registration.Status == RegistrationStatus.Waitlisted
                ? $"Registration placed on waitlist (Position #{registration.WaitlistPosition}). Confirmation #: {registration.ConfirmationNumber}"
                : $"Registration confirmed! Confirmation #: {registration.ConfirmationNumber}";

            TempData["SuccessMessage"] = statusMessage;
            return RedirectToPage("/Registrations/Details", new { id = registration.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadFormDataAsync(eventId);
            return Page();
        }
    }

    private async Task LoadFormDataAsync(int eventId)
    {
        Attendees = await _attendeeService.GetAllForDropdownAsync();
        TicketTypes = await _ticketTypeService.GetByEventIdAsync(eventId);
    }
}
