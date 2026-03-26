using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public class EditModel : PageModel
{
    private readonly IVenueService _venueService;
    public EditModel(IVenueService venueService) => _venueService = venueService;

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
        public string ZipCode { get; set; } = string.Empty;
        [Required, Range(1, int.MaxValue)]
        [Display(Name = "Max Capacity")]
        public int MaxCapacity { get; set; }
        [EmailAddress, Display(Name = "Contact Email")]
        public string? ContactEmail { get; set; }
        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var venue = await _venueService.GetVenueByIdAsync(id);
        if (venue == null) return NotFound();
        Input = new InputModel
        {
            Name = venue.Name, Address = venue.Address, City = venue.City,
            State = venue.State, ZipCode = venue.ZipCode, MaxCapacity = venue.MaxCapacity,
            ContactEmail = venue.ContactEmail, ContactPhone = venue.ContactPhone, Notes = venue.Notes
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid) return Page();
        var venue = await _venueService.GetVenueByIdAsync(id);
        if (venue == null) return NotFound();
        venue.Name = Input.Name; venue.Address = Input.Address; venue.City = Input.City;
        venue.State = Input.State; venue.ZipCode = Input.ZipCode; venue.MaxCapacity = Input.MaxCapacity;
        venue.ContactEmail = Input.ContactEmail; venue.ContactPhone = Input.ContactPhone; venue.Notes = Input.Notes;
        await _venueService.UpdateVenueAsync(venue);
        TempData["SuccessMessage"] = "Venue updated successfully.";
        return RedirectToPage("Index");
    }
}
