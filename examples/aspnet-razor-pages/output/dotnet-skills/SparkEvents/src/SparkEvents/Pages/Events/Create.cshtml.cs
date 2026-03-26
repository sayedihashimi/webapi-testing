using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class CreateModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;
    private readonly IVenueService _venueService;

    public CreateModel(IEventService eventService, ICategoryService categoryService, IVenueService venueService)
    {
        _eventService = eventService;
        _categoryService = categoryService;
        _venueService = venueService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public SelectList CategoryList { get; set; } = null!;
    public SelectList VenueList { get; set; } = null!;

    public class InputModel
    {
        [Required, MaxLength(300)]
        public string Title { get; set; } = string.Empty;
        [Required, MaxLength(5000)]
        public string Description { get; set; } = string.Empty;
        [Required, Display(Name = "Category")]
        public int EventCategoryId { get; set; }
        [Required, Display(Name = "Venue")]
        public int VenueId { get; set; }
        [Required, Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Required, Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        [Required, Display(Name = "Registration Opens")]
        public DateTime RegistrationOpenDate { get; set; }
        [Required, Display(Name = "Registration Closes")]
        public DateTime RegistrationCloseDate { get; set; }
        [Display(Name = "Early Bird Deadline")]
        public DateTime? EarlyBirdDeadline { get; set; }
        [Required, Range(1, int.MaxValue), Display(Name = "Total Capacity")]
        public int TotalCapacity { get; set; }
        [Display(Name = "Featured")]
        public bool IsFeatured { get; set; }
    }

    public async Task OnGetAsync()
    {
        await PopulateListsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Validate dates
        if (Input.EndDate <= Input.StartDate)
            ModelState.AddModelError("Input.EndDate", "End date must be after start date.");
        if (Input.RegistrationCloseDate > Input.StartDate)
            ModelState.AddModelError("Input.RegistrationCloseDate", "Registration must close before or on the start date.");

        // Validate venue capacity
        var venues = await _venueService.GetAllVenuesAsync();
        var venue = venues.FirstOrDefault(v => v.Id == Input.VenueId);
        if (venue != null && Input.TotalCapacity > venue.MaxCapacity)
            ModelState.AddModelError("Input.TotalCapacity", $"Capacity cannot exceed venue max capacity ({venue.MaxCapacity}).");

        if (!ModelState.IsValid) { await PopulateListsAsync(); return Page(); }

        var evt = new Event
        {
            Title = Input.Title, Description = Input.Description,
            EventCategoryId = Input.EventCategoryId, VenueId = Input.VenueId,
            StartDate = Input.StartDate, EndDate = Input.EndDate,
            RegistrationOpenDate = Input.RegistrationOpenDate, RegistrationCloseDate = Input.RegistrationCloseDate,
            EarlyBirdDeadline = Input.EarlyBirdDeadline, TotalCapacity = Input.TotalCapacity,
            IsFeatured = Input.IsFeatured, Status = EventStatus.Draft
        };

        await _eventService.CreateEventAsync(evt);
        TempData["SuccessMessage"] = "Event created successfully. Add ticket types before publishing.";
        return RedirectToPage("Details", new { id = evt.Id });
    }

    private async Task PopulateListsAsync()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        var venues = await _venueService.GetAllVenuesAsync();
        CategoryList = new SelectList(categories, "Id", "Name");
        VenueList = new SelectList(venues.Select(v => new { v.Id, Display = $"{v.Name} (max {v.MaxCapacity})" }), "Id", "Display");
    }
}
