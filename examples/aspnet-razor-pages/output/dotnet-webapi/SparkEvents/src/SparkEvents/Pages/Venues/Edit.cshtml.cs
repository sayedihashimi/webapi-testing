using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public sealed class EditModel : PageModel
{
    private readonly IVenueService _venueService;

    public EditModel(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public int VenueId { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var venue = await _venueService.GetByIdAsync(id, ct);
        if (venue is null)
        {
            return NotFound();
        }

        VenueId = venue.Id;
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

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            VenueId = Input.Id;
            return Page();
        }

        var venue = await _venueService.GetByIdAsync(Input.Id, ct);
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

        await _venueService.UpdateAsync(venue, ct);

        TempData["SuccessMessage"] = "Venue updated successfully.";
        return RedirectToPage("Index");
    }

    public sealed class InputModel
    {
        [Required]
        public int Id { get; set; }

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

        [Range(1, int.MaxValue, ErrorMessage = "Max capacity must be positive.")]
        [Display(Name = "Max Capacity")]
        public int MaxCapacity { get; set; }

        [EmailAddress, MaxLength(254)]
        [Display(Name = "Contact Email")]
        public string? ContactEmail { get; set; }

        [MaxLength(20)]
        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
