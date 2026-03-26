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

    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Venues { get; set; } = new();

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

        [Required, Range(1, int.MaxValue, ErrorMessage = "Capacity must be positive")]
        [Display(Name = "Total Capacity")]
        public int TotalCapacity { get; set; }

        [Display(Name = "Featured")]
        public bool IsFeatured { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadSelectListsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        // Validate dates
        if (Input.EndDate <= Input.StartDate)
        {
            ModelState.AddModelError("Input.EndDate", "End date must be after start date.");
            await LoadSelectListsAsync();
            return Page();
        }
        if (Input.RegistrationCloseDate > Input.StartDate)
        {
            ModelState.AddModelError("Input.RegistrationCloseDate", "Registration close date must be before or on the start date.");
            await LoadSelectListsAsync();
            return Page();
        }

        // Validate capacity against venue
        var venue = await _venueService.GetVenueByIdAsync(Input.VenueId);
        if (venue != null && Input.TotalCapacity > venue.MaxCapacity)
        {
            ModelState.AddModelError("Input.TotalCapacity", $"Capacity cannot exceed venue max capacity of {venue.MaxCapacity}.");
            await LoadSelectListsAsync();
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
            Status = EventStatus.Draft
        };

        await _eventService.CreateEventAsync(evt);
        TempData["SuccessMessage"] = "Event created successfully.";
        return RedirectToPage("Details", new { id = evt.Id });
    }

    private async Task LoadSelectListsAsync()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();

        var venues = await _venueService.GetAllVenuesAsync();
        Venues = venues.Select(v => new SelectListItem($"{v.Name} (Max: {v.MaxCapacity})", v.Id.ToString())).ToList();
    }
}
