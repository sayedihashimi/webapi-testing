using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Data;

namespace SparkEvents.Pages.Attendees;

public sealed class EditModel(SparkEventsDbContext db) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var attendee = await db.Attendees.FindAsync(id);
        if (attendee is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = attendee.Id,
            FirstName = attendee.FirstName,
            LastName = attendee.LastName,
            Email = attendee.Email,
            Phone = attendee.Phone,
            Organization = attendee.Organization,
            DietaryNeeds = attendee.DietaryNeeds
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var attendee = await db.Attendees.FindAsync(Input.Id);
        if (attendee is null)
        {
            return NotFound();
        }

        attendee.FirstName = Input.FirstName;
        attendee.LastName = Input.LastName;
        attendee.Email = Input.Email;
        attendee.Phone = Input.Phone;
        attendee.Organization = Input.Organization;
        attendee.DietaryNeeds = Input.DietaryNeeds;

        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Attendee \"{attendee.FullName}\" updated successfully.";
        return RedirectToPage("Index");
    }

    public sealed class InputModel
    {
        public int Id { get; set; }

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
