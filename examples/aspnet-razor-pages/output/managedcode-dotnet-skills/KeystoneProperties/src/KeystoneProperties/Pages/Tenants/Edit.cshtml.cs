using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public class EditModel(ITenantService tenantService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var tenant = await tenantService.GetTenantByIdAsync(id);
        if (tenant is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = tenant.Id,
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!await tenantService.IsEmailUniqueAsync(Input.Email, Input.Id))
        {
            ModelState.AddModelError("Input.Email", "A tenant with this email address already exists.");
            return Page();
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - Input.DateOfBirth.Year;
        if (Input.DateOfBirth > today.AddYears(-age))
        {
            age--;
        }
        if (age < 18)
        {
            ModelState.AddModelError("Input.DateOfBirth", "Tenant must be at least 18 years old.");
            return Page();
        }

        var tenant = await tenantService.GetTenantByIdAsync(Input.Id);
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
        tenant.UpdatedAt = DateTime.UtcNow;

        await tenantService.UpdateTenantAsync(tenant);
        TempData["SuccessMessage"] = "Tenant updated successfully.";
        return RedirectToPage("/Tenants/Index");
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

        [Required]
        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Date of Birth")]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Emergency Contact Name")]
        public string EmergencyContactName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Emergency Contact Phone")]
        public string EmergencyContactPhone { get; set; } = string.Empty;
    }
}
