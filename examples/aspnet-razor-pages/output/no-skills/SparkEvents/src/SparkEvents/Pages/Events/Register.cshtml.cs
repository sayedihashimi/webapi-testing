using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class RegisterModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IRegistrationService _registrationService;
    private readonly IAttendeeService _attendeeService;
    private readonly ITicketTypeService _ticketTypeService;

    public RegisterModel(IEventService eventService, IRegistrationService registrationService,
        IAttendeeService attendeeService, ITicketTypeService ticketTypeService)
    {
        _eventService = eventService;
        _registrationService = registrationService;
        _attendeeService = attendeeService;
        _ticketTypeService = ticketTypeService;
    }

    public Event Event { get; set; } = null!;
    public List<Attendee> Attendees { get; set; } = new();
    public List<TicketType> TicketTypes { get; set; } = new();
    public bool IsRegistrationOpen { get; set; }
    public bool IsEarlyBird { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Please select an attendee.")]
        [Display(Name = "Attendee")]
        public int AttendeeId { get; set; }

        [Required(ErrorMessage = "Please select a ticket type.")]
        [Display(Name = "Ticket Type")]
        public int TicketTypeId { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Special Requests")]
        public string? SpecialRequests { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        await LoadDataAsync(eventId);
        if (Event == null) return NotFound();
        Input.EventId = eventId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int eventId)
    {
        await LoadDataAsync(eventId);
        if (Event == null) return NotFound();

        if (!ModelState.IsValid) return Page();

        try
        {
            var reg = await _registrationService.RegisterAsync(eventId, Input.AttendeeId, Input.TicketTypeId, Input.SpecialRequests);
            TempData["SuccessMessage"] = $"Registration successful! Confirmation #: {reg.ConfirmationNumber}";
            return RedirectToPage("/Registrations/Details", new { id = reg.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return Page();
        }
    }

    private async Task LoadDataAsync(int eventId)
    {
        var evt = await _eventService.GetEventByIdAsync(eventId);
        if (evt == null) return;
        Event = evt;
        Attendees = await _attendeeService.GetAllAttendeesAsync();
        TicketTypes = await _ticketTypeService.GetTicketTypesForEventAsync(eventId);
        TicketTypes = TicketTypes.Where(t => t.IsActive).ToList();

        var now = DateTime.UtcNow;
        IsRegistrationOpen = now >= evt.RegistrationOpenDate && now <= evt.RegistrationCloseDate
            && evt.Status != EventStatus.Cancelled && evt.Status != EventStatus.Completed && evt.Status != EventStatus.Draft;
        IsEarlyBird = evt.EarlyBirdDeadline.HasValue && now <= evt.EarlyBirdDeadline.Value;
    }
}
