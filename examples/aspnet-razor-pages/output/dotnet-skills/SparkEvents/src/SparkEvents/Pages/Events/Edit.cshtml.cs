using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class EditModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;
    private readonly IVenueService _venueService;

    public EditModel(IEventService eventService, ICategoryService categoryService, IVenueService venueService)
    {
        _eventService = eventService;
        _categoryService = categoryService;
        _venueService = venueService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public int Id { get; set; }
    public int CurrentRegistrations { get; set; }
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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var evt = await _eventService.GetEventByIdAsync(id);
        if (evt == null) return NotFound();
        Id = id;
        CurrentRegistrations = evt.CurrentRegistrations;
        Input = new InputModel
        {
            Title = evt.Title, Description = evt.Description,
            EventCategoryId = evt.EventCategoryId, VenueId = evt.VenueId,
            StartDate = evt.StartDate, EndDate = evt.EndDate,
            RegistrationOpenDate = evt.RegistrationOpenDate, RegistrationCloseDate = evt.RegistrationCloseDate,
            EarlyBirdDeadline = evt.EarlyBirdDeadline, TotalCapacity = evt.TotalCapacity,
            IsFeatured = evt.IsFeatured
        };
        await PopulateListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var evt = await _eventService.GetEventByIdAsync(id);
        if (evt == null) return NotFound();

        if (Input.EndDate <= Input.StartDate)
            ModelState.AddModelError("Input.EndDate", "End date must be after start date.");
        if (Input.TotalCapacity < evt.CurrentRegistrations)
            ModelState.AddModelError("Input.TotalCapacity", $"Cannot reduce capacity below current registrations ({evt.CurrentRegistrations}).");

        var venues = await _venueService.GetAllVenuesAsync();
        var venue = venues.FirstOrDefault(v => v.Id == Input.VenueId);
        if (venue != null && Input.TotalCapacity > venue.MaxCapacity)
            ModelState.AddModelError("Input.TotalCapacity", $"Capacity cannot exceed venue max capacity ({venue.MaxCapacity}).");

        if (!ModelState.IsValid) { Id = id; CurrentRegistrations = evt.CurrentRegistrations; await PopulateListsAsync(); return Page(); }

        evt.Title = Input.Title; evt.Description = Input.Description;
        evt.EventCategoryId = Input.EventCategoryId; evt.VenueId = Input.VenueId;
        evt.StartDate = Input.StartDate; evt.EndDate = Input.EndDate;
        evt.RegistrationOpenDate = Input.RegistrationOpenDate; evt.RegistrationCloseDate = Input.RegistrationCloseDate;
        evt.EarlyBirdDeadline = Input.EarlyBirdDeadline; evt.TotalCapacity = Input.TotalCapacity;
        evt.IsFeatured = Input.IsFeatured;

        await _eventService.UpdateEventAsync(evt);
        TempData["SuccessMessage"] = "Event updated successfully.";
        return RedirectToPage("Details", new { id });
    }

    private async Task PopulateListsAsync()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        var venues = await _venueService.GetAllVenuesAsync();
        CategoryList = new SelectList(categories, "Id", "Name");
        VenueList = new SelectList(venues.Select(v => new { v.Id, Display = $"{v.Name} (max {v.MaxCapacity})" }), "Id", "Display");
    }
}
