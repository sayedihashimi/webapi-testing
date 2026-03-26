using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Attendees;

public class CreateModel(IAttendeeService attendeeService) : PageModel
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

        if (await attendeeService.EmailExistsAsync(Input.Email))
        {
            ModelState.AddModelError("Input.Email", "An attendee with this email already exists.");
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

        await attendeeService.CreateAsync(attendee);

        TempData["SuccessMessage"] = "Attendee created successfully.";
        return RedirectToPage("Index");
    }

    public class InputModel
    {
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
