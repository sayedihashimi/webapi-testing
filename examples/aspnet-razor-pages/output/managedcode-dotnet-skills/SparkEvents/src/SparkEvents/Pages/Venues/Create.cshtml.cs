using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public class CreateModel(IVenueService venueService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

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

        await venueService.CreateAsync(venue);

        TempData["SuccessMessage"] = "Venue created successfully.";
        return RedirectToPage("Index");
    }

    public class InputModel
    {
        [Required]
        [MaxLength(200)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(2)]
        [Display(Name = "State")]
        [RegularExpression(@"^[A-Za-z]{2}$", ErrorMessage = "State must be a 2-letter code (e.g. CA).")]
        public string State { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Max Capacity must be a positive number.")]
        [Display(Name = "Max Capacity")]
        public int MaxCapacity { get; set; }

        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string? ContactEmail { get; set; }

        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
