using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Attendees;

public class EditModel(IAttendeeService attendeeService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var attendee = await attendeeService.GetByIdAsync(id);
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

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var attendee = await attendeeService.GetByIdAsync(id);
        if (attendee is null)
        {
            return NotFound();
        }

        if (await attendeeService.EmailExistsAsync(Input.Email, id))
        {
            ModelState.AddModelError("Input.Email", "An attendee with this email already exists.");
            return Page();
        }

        attendee.FirstName = Input.FirstName;
        attendee.LastName = Input.LastName;
        attendee.Email = Input.Email;
        attendee.Phone = Input.Phone;
        attendee.Organization = Input.Organization;
        attendee.DietaryNeeds = Input.DietaryNeeds;

        await attendeeService.UpdateAsync(attendee);

        TempData["SuccessMessage"] = "Attendee updated successfully.";
        return RedirectToPage("Index");
    }

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [MaxLength(200)]
        [Display(Name = "Organization")]
        public string? Organization { get; set; }

        [MaxLength(500)]
        [Display(Name = "Dietary Needs")]
        public string? DietaryNeeds { get; set; }
    }
}
