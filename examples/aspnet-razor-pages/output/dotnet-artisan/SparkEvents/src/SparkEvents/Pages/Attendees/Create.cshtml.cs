using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Pages.Attendees;

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

        var attendee = new Attendee
        {
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            Email = Input.Email,
            Phone = Input.Phone,
            Organization = Input.Organization,
            DietaryNeeds = Input.DietaryNeeds
        };

        db.Attendees.Add(attendee);
        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Attendee \"{attendee.FullName}\" created successfully.";
        return RedirectToPage("Index");
    }

    public sealed class InputModel
    {
        [Required, MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Organization { get; set; }

        [MaxLength(500)]
        [Display(Name = "Dietary Needs")]
        public string? DietaryNeeds { get; set; }
    }
}
