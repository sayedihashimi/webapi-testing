using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class CreateModel(IEventService eventService, SparkEventsDbContext db) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Categories { get; set; } = null!;
    public SelectList Venues { get; set; } = null!;

    public sealed class InputModel : IValidatableObject
    {
        [Required, MaxLength(300)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(5000)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Category")]
        public int EventCategoryId { get; set; }

        [Display(Name = "Featured")]
        public bool IsFeatured { get; set; }

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
        [Display(Name = "Registration Opens")]
        public DateTime RegistrationOpenDate { get; set; }

        [Required]
        [Display(Name = "Registration Closes")]
        public DateTime RegistrationCloseDate { get; set; }

        [Display(Name = "Early Bird Deadline")]
        public DateTime? EarlyBirdDeadline { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1.")]
        [Display(Name = "Total Capacity")]
        public int TotalCapacity { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult("End date must be after start date.", [nameof(EndDate)]);
            }

            if (RegistrationCloseDate > StartDate)
            {
                yield return new ValidationResult("Registration must close on or before the start date.", [nameof(RegistrationCloseDate)]);
            }
        }
    }

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDropdownsAsync();

        // Venue capacity validation
        var venue = await db.Venues.FindAsync(Input.VenueId);
        if (venue is not null && Input.TotalCapacity > venue.MaxCapacity)
        {
            ModelState.AddModelError("Input.TotalCapacity", $"Capacity cannot exceed venue maximum of {venue.MaxCapacity}.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var evt = new Event
        {
            Title = Input.Title,
            Description = Input.Description,
            EventCategoryId = Input.EventCategoryId,
            VenueId = Input.VenueId,
            StartDate = Input.StartDate,
            EndDate = Input.EndDate,
            RegistrationOpenDate = Input.RegistrationOpenDate,
            RegistrationCloseDate = Input.RegistrationCloseDate,
            EarlyBirdDeadline = Input.EarlyBirdDeadline,
            TotalCapacity = Input.TotalCapacity,
            IsFeatured = Input.IsFeatured,
            Status = EventStatus.Draft,
        };

        await eventService.CreateEventAsync(evt);

        TempData["StatusMessage"] = "Event created successfully.";
        return RedirectToPage("Details", new { id = evt.Id });
    }

    private async Task LoadDropdownsAsync()
    {
        var categories = await db.EventCategories.OrderBy(c => c.Name).AsNoTracking().ToListAsync();
        Categories = new SelectList(categories, nameof(EventCategory.Id), nameof(EventCategory.Name));

        var venues = await db.Venues.OrderBy(v => v.Name).AsNoTracking().ToListAsync();
        Venues = new SelectList(
            venues.Select(v => new { v.Id, Display = $"{v.Name} (Max: {v.MaxCapacity})" }),
            "Id",
            "Display");
    }
}
