using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Registrations;

public class DetailsModel : PageModel
{
    private readonly IRegistrationService _registrationService;
    public DetailsModel(IRegistrationService registrationService) => _registrationService = registrationService;

    public Registration? Registration { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Registration = await _registrationService.GetRegistrationByIdAsync(id);
        if (Registration == null) return NotFound();
        return Page();
    }
}
