using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Attendees;

public class CreateModel : PageModel
{
    private readonly IAttendeeService _attendeeService;
    public CreateModel(IAttendeeService attendeeService) => _attendeeService = attendeeService;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(100), Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        [Required, MaxLength(100), Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;
        [MaxLength(20)]
        public string? Phone { get; set; }
        [MaxLength(200)]
        public string? Organization { get; set; }
        [MaxLength(500), Display(Name = "Dietary Needs")]
        public string? DietaryNeeds { get; set; }
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var existing = await _attendeeService.GetAttendeeByEmailAsync(Input.Email);
        if (existing != null)
        {
            ModelState.AddModelError("Input.Email", "An attendee with this email already exists.");
            return Page();
        }

        var attendee = new Attendee
        {
            FirstName = Input.FirstName, LastName = Input.LastName, Email = Input.Email,
            Phone = Input.Phone, Organization = Input.Organization, DietaryNeeds = Input.DietaryNeeds
        };
        await _attendeeService.CreateAttendeeAsync(attendee);
        TempData["SuccessMessage"] = "Attendee added successfully.";
        return RedirectToPage("Index");
    }
}
