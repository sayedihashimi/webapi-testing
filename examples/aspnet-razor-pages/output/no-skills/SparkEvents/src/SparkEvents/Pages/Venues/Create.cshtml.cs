using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public class CreateModel : PageModel
{
    private readonly IVenueService _venueService;

    public CreateModel(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(2)]
        public string State { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; } = string.Empty;

        [Required, Range(1, int.MaxValue, ErrorMessage = "Max capacity must be positive")]
        [Display(Name = "Max Capacity")]
        public int MaxCapacity { get; set; }

        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string? ContactEmail { get; set; }

        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var venue = new Venue
        {
            Name = Input.Name,
            Address = Input.Address,
            City = Input.City,
            State = Input.State,
            ZipCode = Input.ZipCode,
            MaxCapacity = Input.MaxCapacity,
            ContactEmail = Input.ContactEmail,
            ContactPhone = Input.ContactPhone,
            Notes = Input.Notes
        };

        await _venueService.CreateVenueAsync(venue);
        TempData["SuccessMessage"] = "Venue created successfully.";
        return RedirectToPage("Index");
    }
}
