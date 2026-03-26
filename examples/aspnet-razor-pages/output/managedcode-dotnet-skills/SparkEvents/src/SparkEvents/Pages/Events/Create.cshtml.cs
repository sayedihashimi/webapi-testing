using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class CreateModel(
    IEventService eventService,
    ICategoryService categoryService,
    IVenueService venueService) : PageModel
{
    [BindProperty]
    public EventInputModel Input { get; set; } = new();

    public IReadOnlyList<EventCategory> Categories { get; set; } = [];
    public IReadOnlyList<Venue> Venues { get; set; } = [];

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDropdownsAsync();

        if (!ValidateDates())
        {
            return Page();
        }

        if (!await ValidateCapacityAsync())
        {
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ev = new Event
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
            Status = EventStatus.Draft
        };

        var created = await eventService.CreateAsync(ev);
        TempData["SuccessMessage"] = "Event created successfully.";
        return RedirectToPage("Details", new { id = created.Id });
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
