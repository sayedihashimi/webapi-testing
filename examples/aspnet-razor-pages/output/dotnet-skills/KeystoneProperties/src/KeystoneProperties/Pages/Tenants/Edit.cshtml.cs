using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public class EditModel : PageModel
{
    private readonly ITenantService _tenantService;

    public EditModel(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public int TenantId { get; set; }

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

        public bool IsActive { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var tenant = await _tenantService.GetByIdAsync(id);
        if (tenant == null)
        {
            return NotFound();
        }

        TenantId = id;
        Input = new InputModel
        {
            Id = tenant.Id,
            FirstName = tenant.FirstName,
            LastName = tenant.LastName,
            Email = tenant.Email,
            Phone = tenant.Phone,
            DateOfBirth = tenant.DateOfBirth,
            EmergencyContactName = tenant.EmergencyContactName,
            EmergencyContactPhone = tenant.EmergencyContactPhone,
            IsActive = tenant.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            TenantId = Input.Id;
            return Page();
        }

        // Validate age (must be at least 18)
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - Input.DateOfBirth.Year;
        if (Input.DateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        if (age < 18)
        {
            ModelState.AddModelError("Input.DateOfBirth", "Tenant must be at least 18 years old.");
            TenantId = Input.Id;
            return Page();
        }

        // Check email uniqueness excluding current tenant
        if (!await _tenantService.IsEmailUniqueAsync(Input.Email, Input.Id))
        {
            ModelState.AddModelError("Input.Email", "A tenant with this email address already exists.");
            TenantId = Input.Id;
            return Page();
        }

        var tenant = await _tenantService.GetByIdAsync(Input.Id);
        if (tenant == null)
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

        await _tenantService.UpdateAsync(tenant);

        TempData["SuccessMessage"] = $"Tenant {tenant.FullName} was updated successfully.";
        return RedirectToPage("Details", new { id = Input.Id });
    }
}
