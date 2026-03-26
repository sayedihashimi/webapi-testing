using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class RegisterModel(
    SparkEventsDbContext db,
    IRegistrationService registrationService,
    IEventService eventService) : PageModel
{
    public Event Event { get; set; } = null!;
    public List<TicketTypeOption> TicketOptions { get; set; } = [];
    public SelectList AttendeeList { get; set; } = null!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        if (!await LoadEventDataAsync(eventId))
        {
            return NotFound();
        }

        Input.EventId = eventId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await LoadEventDataAsync(Input.EventId))
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var registration = await registrationService.RegisterAsync(
                Input.EventId,
                Input.AttendeeId,
                Input.TicketTypeId,
                Input.SpecialRequests);

            TempData["SuccessMessage"] = registration.Status == RegistrationStatus.Waitlisted
                ? $"You have been added to the waitlist (position #{registration.WaitlistPosition}). Confirmation #: {registration.ConfirmationNumber}"
                : $"Registration confirmed! Confirmation #: {registration.ConfirmationNumber}";

            return RedirectToPage("/Registrations/Details", new { id = registration.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return Page();
        }
    }

    private async Task<bool> LoadEventDataAsync(int eventId)
    {
        var evt = await eventService.GetEventByIdAsync(eventId);
        if (evt is null)
        {
            return false;
        }

        Event = evt;

        TicketOptions = evt.TicketTypes
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .Select(t => new TicketTypeOption
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Price = registrationService.CalculatePrice(t, evt),
                RegularPrice = t.Price,
                IsEarlyBird = t.EarlyBirdPrice.HasValue && evt.EarlyBirdDeadline.HasValue && DateTime.UtcNow <= evt.EarlyBirdDeadline.Value,
                Remaining = t.Quantity - t.QuantitySold
            })
            .ToList();

        var attendees = await db.Attendees
            .AsNoTracking()
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .ToListAsync();

        AttendeeList = new SelectList(
            attendees.Select(a => new { a.Id, Name = $"{a.LastName}, {a.FirstName} ({a.Email})" }),
            "Id", "Name");

        return true;
    }

    public sealed class InputModel
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

    public sealed class TicketTypeOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal RegularPrice { get; set; }
        public bool IsEarlyBird { get; set; }
        public int Remaining { get; set; }
    }
}
