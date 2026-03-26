using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Data;

namespace SparkEvents.Pages.Venues;

public sealed class EditModel(SparkEventsDbContext db) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var venue = await db.Venues.FindAsync(id);
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var venue = await db.Venues.FindAsync(Input.Id);
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

        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Venue \"{venue.Name}\" updated successfully.";
        return RedirectToPage("Index");
    }

    public sealed class InputModel
    {
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
