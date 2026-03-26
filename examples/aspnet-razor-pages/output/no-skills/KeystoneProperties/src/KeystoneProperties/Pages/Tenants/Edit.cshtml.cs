using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public class EditModel : PageModel
{
    private readonly ITenantService _tenantService;
    public EditModel(ITenantService tenantService) { _tenantService = tenantService; }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(100), Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        [Required, MaxLength(100), Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, Phone] public string Phone { get; set; } = string.Empty;
        [Required, Display(Name = "Date of Birth")]
        public DateOnly DateOfBirth { get; set; }
        [Required, MaxLength(200), Display(Name = "Emergency Contact Name")]
        public string EmergencyContactName { get; set; } = string.Empty;
        [Required, Phone, Display(Name = "Emergency Contact Phone")]
        public string EmergencyContactPhone { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var tenant = await _tenantService.GetByIdAsync(id);
        if (tenant == null) return NotFound();
        Id = id;
        Input = new InputModel
        {
            FirstName = tenant.FirstName, LastName = tenant.LastName,
            Email = tenant.Email, Phone = tenant.Phone,
            DateOfBirth = tenant.DateOfBirth,
            EmergencyContactName = tenant.EmergencyContactName,
            EmergencyContactPhone = tenant.EmergencyContactPhone
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var tenant = await _tenantService.GetByIdAsync(Id);
        if (tenant == null) return NotFound();

        if (!await _tenantService.IsEmailUniqueAsync(Input.Email, Id))
        {
            ModelState.AddModelError("Input.Email", "This email is already in use.");
            return Page();
        }

        tenant.FirstName = Input.FirstName;
        tenant.LastName = Input.LastName;
        tenant.Email = Input.Email;
        tenant.Phone = Input.Phone;
        tenant.DateOfBirth = Input.DateOfBirth;
        tenant.EmergencyContactName = Input.EmergencyContactName;
        tenant.EmergencyContactPhone = Input.EmergencyContactPhone;

        await _tenantService.UpdateAsync(tenant);
        TempData["SuccessMessage"] = "Tenant updated successfully.";
        return RedirectToPage("Details", new { id = Id });
    }
}
