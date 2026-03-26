using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class EditModel(
    IEventService eventService,
    ICategoryService categoryService,
    IVenueService venueService) : PageModel
{
    [BindProperty]
    public EventInputModel Input { get; set; } = new();

    [BindProperty]
    public int EventId { get; set; }

    public IReadOnlyList<EventCategory> Categories { get; set; } = [];
    public IReadOnlyList<Venue> Venues { get; set; } = [];
    public string? CapacityWarning { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var ev = await eventService.GetByIdAsync(id);
        if (ev is null)
        {
            return NotFound();
        }

        if (ev.Status is EventStatus.Completed or EventStatus.Cancelled)
        {
            TempData["ErrorMessage"] = $"Cannot edit a {ev.Status.ToString().ToLowerInvariant()} event.";
            return RedirectToPage("Details", new { id });
        }

        EventId = id;
        await LoadDropdownsAsync();
        PopulateInput(ev);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var ev = await eventService.GetByIdAsync(EventId);
        if (ev is null)
        {
            return NotFound();
        }

        if (ev.Status is EventStatus.Completed or EventStatus.Cancelled)
        {
            TempData["ErrorMessage"] = $"Cannot edit a {ev.Status.ToString().ToLowerInvariant()} event.";
            return RedirectToPage("Details", new { id = EventId });
        }

        await LoadDropdownsAsync();

        if (!ValidateDates())
        {
            return Page();
        }

        if (!await ValidateCapacityAsync())
        {
            return Page();
        }

        if (Input.TotalCapacity < ev.CurrentRegistrations)
        {
            CapacityWarning = $"Warning: Reducing capacity below current registrations ({ev.CurrentRegistrations}). Some registrations may be affected.";
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        ev.Title = Input.Title;
        ev.Description = Input.Description;
        ev.EventCategoryId = Input.EventCategoryId;
        ev.VenueId = Input.VenueId;
        ev.StartDate = Input.StartDate;
        ev.EndDate = Input.EndDate;
        ev.RegistrationOpenDate = Input.RegistrationOpenDate;
        ev.RegistrationCloseDate = Input.RegistrationCloseDate;
        ev.EarlyBirdDeadline = Input.EarlyBirdDeadline;
        ev.TotalCapacity = Input.TotalCapacity;
        ev.IsFeatured = Input.IsFeatured;
        ev.UpdatedAt = DateTime.UtcNow;

        await eventService.UpdateAsync(ev);
        TempData["SuccessMessage"] = "Event updated successfully.";
        return RedirectToPage("Details", new { id = EventId });
    }

    private void PopulateInput(Event ev)
    {
        Input = new EventInputModel
        {
            Title = ev.Title,
            Description = ev.Description,
            EventCategoryId = ev.EventCategoryId,
            VenueId = ev.VenueId,
            StartDate = ev.StartDate,
            EndDate = ev.EndDate,
            RegistrationOpenDate = ev.RegistrationOpenDate,
            RegistrationCloseDate = ev.RegistrationCloseDate,
            EarlyBirdDeadline = ev.EarlyBirdDeadline,
            TotalCapacity = ev.TotalCapacity,
            IsFeatured = ev.IsFeatured
        };
    }

    private bool ValidateDates()
    {
        var valid = true;

        if (Input.EndDate <= Input.StartDate)
        {
            ModelState.AddModelError("Input.EndDate", "End date must be after start date.");
            valid = false;
        }

        if (Input.RegistrationCloseDate > Input.StartDate)
        {
            ModelState.AddModelError("Input.RegistrationCloseDate", "Registration close date must be on or before the start date.");
            valid = false;
        }

        return valid;
    }

    private async Task<bool> ValidateCapacityAsync()
    {
        var venue = await venueService.GetByIdAsync(Input.VenueId);
        if (venue is not null && Input.TotalCapacity > venue.MaxCapacity)
        {
            ModelState.AddModelError("Input.TotalCapacity",
                $"Capacity cannot exceed the venue's maximum capacity of {venue.MaxCapacity}.");
            return false;
        }

        return true;
    }

    private async Task LoadDropdownsAsync()
    {
        Categories = await categoryService.GetAllAsync();
        Venues = await venueService.GetAllAsync();
    }

    public class EventInputModel
    {
        [Required]
        [MaxLength(300)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(5000)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Category")]
        public int EventCategoryId { get; set; }

        [Required]
        [Display(Name = "Venue")]
        public int VenueId { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "Registration Open Date")]
        public DateTime RegistrationOpenDate { get; set; }

        [Required]
        [Display(Name = "Registration Close Date")]
        public DateTime RegistrationCloseDate { get; set; }

        [Display(Name = "Early Bird Deadline")]
        public DateTime? EarlyBirdDeadline { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1.")]
        [Display(Name = "Total Capacity")]
        public int TotalCapacity { get; set; }

        [Display(Name = "Featured")]
        public bool IsFeatured { get; set; }
    }
}
