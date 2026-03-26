using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public sealed class EditModel(ITenantService tenantService) : PageModel
{
    [BindProperty]
    public TenantInputModel Input { get; set; } = new();

    public int TenantId { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var tenant = await tenantService.GetByIdAsync(id);
        if (tenant is null)
        {
            return NotFound();
        }

        TenantId = id;
        Input = new TenantInputModel
        {
            FirstName = tenant.FirstName,
            LastName = tenant.LastName,
            Email = tenant.Email,
            Phone = tenant.Phone,
            DateOfBirth = tenant.DateOfBirth,
            EmergencyContactName = tenant.EmergencyContactName,
            EmergencyContactPhone = tenant.EmergencyContactPhone
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            TenantId = id;
            return Page();
        }

        var age = CalculateAge(Input.DateOfBirth);
        if (age < 18)
        {
            ModelState.AddModelError("Input.DateOfBirth", "Tenant must be at least 18 years old.");
            TenantId = id;
            return Page();
        }

        var tenant = await tenantService.GetByIdAsync(id);
        if (tenant is null)
        {
            return NotFound();
        }

        tenant.FirstName = Input.FirstName;
        tenant.LastName = Input.LastName;
        tenant.Email = Input.Email;
        tenant.Phone = Input.Phone;
        tenant.DateOfBirth = Input.DateOfBirth;
        tenant.EmergencyContactName = Input.EmergencyContactName;
        tenant.EmergencyContactPhone = Input.EmergencyContactPhone;

        await tenantService.UpdateAsync(tenant);

        TempData["SuccessMessage"] = $"Tenant {tenant.FullName} updated successfully.";
        return RedirectToPage("Details", new { id });
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
