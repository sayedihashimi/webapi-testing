using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public class EditModel(IVenueService venueService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var venue = await venueService.GetByIdAsync(id);
        if (venue is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = venue.Id,
            Name = venue.Name,
            Address = venue.Address,
            City = venue.City,
            State = venue.State,
            ZipCode = venue.ZipCode,
            MaxCapacity = venue.MaxCapacity,
            ContactEmail = venue.ContactEmail,
            ContactPhone = venue.ContactPhone,
            Notes = venue.Notes
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var venue = await venueService.GetByIdAsync(id);
        if (venue is null)
        {
            return NotFound();
        }

        venue.Name = Input.Name;
        venue.Address = Input.Address;
        venue.City = Input.City;
        venue.State = Input.State;
        venue.ZipCode = Input.ZipCode;
        venue.MaxCapacity = Input.MaxCapacity;
        venue.ContactEmail = Input.ContactEmail;
        venue.ContactPhone = Input.ContactPhone;
        venue.Notes = Input.Notes;

        await venueService.UpdateAsync(venue);

        TempData["SuccessMessage"] = "Venue updated successfully.";
        return RedirectToPage("Index");
    }

    public class InputModel
    {
        public int Id { get; set; }

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
