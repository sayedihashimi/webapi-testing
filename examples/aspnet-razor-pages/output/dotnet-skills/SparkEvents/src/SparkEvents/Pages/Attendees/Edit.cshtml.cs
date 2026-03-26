using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Services;

namespace SparkEvents.Pages.Attendees;

public class EditModel : PageModel
{
    private readonly IAttendeeService _attendeeService;
    public EditModel(IAttendeeService attendeeService) => _attendeeService = attendeeService;

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public int Id { get; set; }

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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var attendee = await _attendeeService.GetAttendeeByIdAsync(id);
        if (attendee == null) return NotFound();
        Id = id;
        Input = new InputModel
        {
            FirstName = attendee.FirstName, LastName = attendee.LastName, Email = attendee.Email,
            Phone = attendee.Phone, Organization = attendee.Organization, DietaryNeeds = attendee.DietaryNeeds
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid) { Id = id; return Page(); }
        var attendee = await _attendeeService.GetAttendeeByIdAsync(id);
        if (attendee == null) return NotFound();
        attendee.FirstName = Input.FirstName; attendee.LastName = Input.LastName; attendee.Email = Input.Email;
        attendee.Phone = Input.Phone; attendee.Organization = Input.Organization; attendee.DietaryNeeds = Input.DietaryNeeds;
        await _attendeeService.UpdateAttendeeAsync(attendee);
        TempData["SuccessMessage"] = "Attendee updated successfully.";
        return RedirectToPage("Details", new { id });
    }
}
