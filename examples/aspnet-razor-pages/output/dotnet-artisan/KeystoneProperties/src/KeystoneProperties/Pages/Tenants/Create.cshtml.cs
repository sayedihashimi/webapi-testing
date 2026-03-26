using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public sealed class CreateModel(ITenantService tenantService) : PageModel
{
    [BindProperty]
    public TenantInputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var age = CalculateAge(Input.DateOfBirth);
        if (age < 18)
        {
            ModelState.AddModelError("Input.DateOfBirth", "Tenant must be at least 18 years old.");
            return Page();
        }

        var tenant = new Tenant
        {
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            Email = Input.Email,
            Phone = Input.Phone,
            DateOfBirth = Input.DateOfBirth,
            EmergencyContactName = Input.EmergencyContactName,
            EmergencyContactPhone = Input.EmergencyContactPhone
        };

        await tenantService.CreateAsync(tenant);

        TempData["SuccessMessage"] = $"Tenant {tenant.FullName} created successfully.";
        return RedirectToPage("Details", new { id = tenant.Id });
    }

    private static int CalculateAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age))
        {
            age--;
        }
        return age;
    }

    public sealed class TenantInputModel
    {
        [Required, MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateOnly DateOfBirth { get; set; }

        [Required, MaxLength(200)]
        [Display(Name = "Emergency Contact Name")]
        public string EmergencyContactName { get; set; } = string.Empty;

        [Required, Phone]
        [Display(Name = "Emergency Contact Phone")]
        public string EmergencyContactPhone { get; set; } = string.Empty;
    }
}
