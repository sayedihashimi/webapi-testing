using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SparkEvents.Models;
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
    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Venues { get; set; } = new();
    public int CurrentRegistrations { get; set; }

    public class InputModel
    {
        public int Id { get; set; }
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
        [Required, Range(1, int.MaxValue)]
        [Display(Name = "Total Capacity")]
        public int TotalCapacity { get; set; }
        [Display(Name = "Featured")]
        public bool IsFeatured { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var evt = await _eventService.GetEventByIdAsync(id);
        if (evt == null) return NotFound();

        CurrentRegistrations = evt.CurrentRegistrations;
        Input = new InputModel
        {
            Id = evt.Id, Title = evt.Title, Description = evt.Description,
            EventCategoryId = evt.EventCategoryId, VenueId = evt.VenueId,
            StartDate = evt.StartDate, EndDate = evt.EndDate,
            RegistrationOpenDate = evt.RegistrationOpenDate,
            RegistrationCloseDate = evt.RegistrationCloseDate,
            EarlyBirdDeadline = evt.EarlyBirdDeadline,
            TotalCapacity = evt.TotalCapacity, IsFeatured = evt.IsFeatured
        };

        await LoadSelectListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await LoadSelectListsAsync(); return Page(); }

        if (Input.EndDate <= Input.StartDate)
        {
            ModelState.AddModelError("Input.EndDate", "End date must be after start date.");
            await LoadSelectListsAsync(); return Page();
        }

        var venue = await _venueService.GetVenueByIdAsync(Input.VenueId);
        if (venue != null && Input.TotalCapacity > venue.MaxCapacity)
        {
            ModelState.AddModelError("Input.TotalCapacity", $"Cannot exceed venue max capacity of {venue.MaxCapacity}.");
            await LoadSelectListsAsync(); return Page();
        }

        var evt = await _eventService.GetEventByIdAsync(Input.Id);
        if (evt == null) return NotFound();

        evt.Title = Input.Title; evt.Description = Input.Description;
        evt.EventCategoryId = Input.EventCategoryId; evt.VenueId = Input.VenueId;
        evt.StartDate = Input.StartDate; evt.EndDate = Input.EndDate;
        evt.RegistrationOpenDate = Input.RegistrationOpenDate;
        evt.RegistrationCloseDate = Input.RegistrationCloseDate;
        evt.EarlyBirdDeadline = Input.EarlyBirdDeadline;
        evt.TotalCapacity = Input.TotalCapacity; evt.IsFeatured = Input.IsFeatured;

        await _eventService.UpdateEventAsync(evt);
        TempData["SuccessMessage"] = "Event updated successfully.";
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
