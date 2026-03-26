using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Pages.Venues;

public sealed class CreateModel(SparkEventsDbContext db) : PageModel
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

        db.Venues.Add(venue);
        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Venue \"{venue.Name}\" created successfully.";
        return RedirectToPage("Index");
    }

    public sealed class InputModel
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

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Max capacity must be a positive number.")]
        [Display(Name = "Max Capacity")]
        public int MaxCapacity { get; set; }

        [EmailAddress, MaxLength(200)]
        [Display(Name = "Contact Email")]
        public string? ContactEmail { get; set; }

        [MaxLength(20)]
        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
