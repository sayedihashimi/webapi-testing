using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public sealed class EditModel(ITenantService tenantService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100), Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100), Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required, Display(Name = "Date of Birth")]
        public DateOnly DateOfBirth { get; set; }

        [Required, MaxLength(200), Display(Name = "Emergency Contact Name")]
        public string EmergencyContactName { get; set; } = string.Empty;

        [Required, Display(Name = "Emergency Contact Phone")]
        public string EmergencyContactPhone { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var tenant = await tenantService.GetByIdAsync(id, ct);
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

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (Input.DateOfBirth > DateOnly.FromDateTime(DateTime.Today.AddYears(-18)))
        {
            ModelState.AddModelError("Input.DateOfBirth", "Tenant must be at least 18 years old.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var tenant = await tenantService.GetByIdAsync(Input.Id, ct);
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

        await tenantService.UpdateAsync(tenant, ct);
        TempData["SuccessMessage"] = "Tenant updated successfully.";
        return RedirectToPage("Details", new { id = Input.Id });
    }
}
