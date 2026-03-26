using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Attendees;

public class CreateModel : PageModel
{
    private readonly IAttendeeService _attendeeService;

    public CreateModel(IAttendeeService attendeeService)
    {
        _attendeeService = attendeeService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Organization { get; set; }

        [MaxLength(500)]
        [Display(Name = "Dietary Needs")]
        public string? DietaryNeeds { get; set; }
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var attendee = new Attendee
        {
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            Email = Input.Email,
            Phone = Input.Phone,
            Organization = Input.Organization,
            DietaryNeeds = Input.DietaryNeeds
        };

        try
        {
            await _attendeeService.CreateAttendeeAsync(attendee);
            TempData["SuccessMessage"] = "Attendee created successfully.";
            return RedirectToPage("Details", new { id = attendee.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "An attendee with this email may already exist.");
            return Page();
        }
    }
}
