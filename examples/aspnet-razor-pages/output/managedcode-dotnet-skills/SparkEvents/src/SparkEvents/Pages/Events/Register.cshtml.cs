using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class RegisterModel(
    IEventService eventService,
    IAttendeeService attendeeService,
    ITicketTypeService ticketTypeService,
    IRegistrationService registrationService) : PageModel
{
    public Event Event { get; set; } = null!;

    public List<Attendee> Attendees { get; set; } = [];

    public List<TicketType> TicketTypes { get; set; } = [];

    public bool RegistrationOpen { get; set; }

    public string? WindowMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var ev = await eventService.GetByIdAsync(eventId);
        if (ev is null)
        {
            return NotFound();
        }

        Event = ev;
        await LoadFormDataAsync(eventId);
        CheckRegistrationWindow();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int eventId)
    {
        var ev = await eventService.GetByIdAsync(eventId);
        if (ev is null)
        {
            return NotFound();
        }

        Event = ev;
        CheckRegistrationWindow();

        if (!RegistrationOpen)
        {
            await LoadFormDataAsync(eventId);
            return Page();
        }

        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(eventId);
            return Page();
        }

        try
        {
            var registration = await registrationService.RegisterAsync(
                eventId, Input.AttendeeId, Input.TicketTypeId, Input.SpecialRequests);

            TempData["SuccessMessage"] = $"Registration successful! Confirmation number: {registration.ConfirmationNumber}";
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
        Attendees = await attendeeService.GetAllAsync();
        TicketTypes = await ticketTypeService.GetByEventIdAsync(eventId);
    }

    private void CheckRegistrationWindow()
    {
        var now = DateTime.UtcNow;

        if (now < Event.RegistrationOpenDate)
        {
            RegistrationOpen = false;
            WindowMessage = $"Registration opens on {Event.RegistrationOpenDate:MMMM dd, yyyy 'at' h:mm tt}.";
        }
        else if (now > Event.RegistrationCloseDate)
        {
            RegistrationOpen = false;
            WindowMessage = "Registration is closed for this event.";
        }
        else
        {
            RegistrationOpen = true;
        }
    }

    public class InputModel
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
}
