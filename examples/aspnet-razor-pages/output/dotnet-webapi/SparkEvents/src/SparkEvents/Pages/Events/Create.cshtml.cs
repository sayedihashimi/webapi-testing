using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class CreateModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IEventCategoryService _categoryService;
    private readonly IVenueService _venueService;

    public CreateModel(IEventService eventService, IEventCategoryService categoryService, IVenueService venueService)
    {
        _eventService = eventService;
        _categoryService = categoryService;
        _venueService = venueService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> Categories { get; set; } = [];
    public List<SelectListItem> Venues { get; set; } = [];
    public List<Venue> VenueList { get; set; } = [];

    public sealed class InputModel
    {
        [Required, MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(5000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int EventCategoryId { get; set; }

        [Required(ErrorMessage = "Please select a venue.")]
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
        [Range(1, int.MaxValue, ErrorMessage = "Total capacity must be a positive number.")]
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
                yield return new ValidationResult(
                    "Registration close date must be on or before the start date.",
                    [nameof(RegistrationCloseDate)]);
            }
        }
    }

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadDropdownsAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadDropdownsAsync(ct);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Custom validation
        var validationErrors = Input.Validate(new ValidationContext(Input));
        foreach (var error in validationErrors)
        {
            foreach (var member in error.MemberNames)
            {
                ModelState.AddModelError($"Input.{member}", error.ErrorMessage!);
            }
        }

        // Validate capacity against venue
        var venue = await _venueService.GetByIdAsync(Input.VenueId, ct);
        if (venue is null)
        {
            ModelState.AddModelError("Input.VenueId", "Selected venue not found.");
        }
        else if (Input.TotalCapacity > venue.MaxCapacity)
        {
            ModelState.AddModelError("Input.TotalCapacity",
                $"Total capacity cannot exceed venue max capacity of {venue.MaxCapacity}.");
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
            Status = EventStatus.Draft
        };

        await _eventService.CreateAsync(evt, ct);

        TempData["SuccessMessage"] = "Event created successfully!";
        return RedirectToPage("Details", new { id = evt.Id });
    }

    private async Task LoadDropdownsAsync(CancellationToken ct)
    {
        var categories = await _categoryService.GetAllForDropdownAsync(ct);
        Categories = categories
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToList();

        VenueList = await _venueService.GetAllForDropdownAsync(ct);
        Venues = VenueList
            .Select(v => new SelectListItem($"{v.Name} (Max: {v.MaxCapacity})", v.Id.ToString()))
            .ToList();
    }
}
