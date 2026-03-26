using System.ComponentModel.DataAnnotations;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Departments;

public class EditModel(IDepartmentService departmentService, IEmployeeService employeeService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> ParentDepartmentOptions { get; set; } = [];
    public List<SelectListItem> ManagerOptions { get; set; } = [];

    public class InputModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ParentDepartmentId { get; set; }
        public int? ManagerId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var dept = await departmentService.GetByIdAsync(id);
        if (dept is null) return NotFound();

        Input = new InputModel
        {
            Id = dept.Id,
            Name = dept.Name,
            Code = dept.Code,
            Description = dept.Description,
            ParentDepartmentId = dept.ParentDepartmentId,
            ManagerId = dept.ManagerId,
            IsActive = dept.IsActive
        };

        await LoadOptionsAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync(Input.Id);
            return Page();
        }

        try
        {
            var dept = await departmentService.GetByIdAsync(Input.Id);
            if (dept is null) return NotFound();

            // Validate hierarchy
            if (!await departmentService.ValidateHierarchy(Input.Id, Input.ParentDepartmentId))
            {
                ModelState.AddModelError("", "Invalid department hierarchy: circular reference detected.");
                await LoadOptionsAsync(Input.Id);
                return Page();
            }

            dept.Name = Input.Name;
            dept.Code = Input.Code.ToUpperInvariant();
            dept.Description = Input.Description;
            dept.ParentDepartmentId = Input.ParentDepartmentId;
            dept.ManagerId = Input.ManagerId;
            dept.IsActive = Input.IsActive;

            await departmentService.UpdateAsync(dept);
            TempData["SuccessMessage"] = $"Department '{dept.Name}' updated successfully.";
            return RedirectToPage("Details", new { id = dept.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadOptionsAsync(Input.Id);
            return Page();
        }
    }

    private async Task LoadOptionsAsync(int departmentId)
    {
        var departments = await departmentService.GetAllAsync();
        ParentDepartmentOptions = departments
            .Where(d => d.Id != departmentId)
            .Select(d => new SelectListItem(d.Name, d.Id.ToString()))
            .ToList();

        var employees = await employeeService.GetByDepartmentAsync(departmentId);
        ManagerOptions = employees
            .Select(e => new SelectListItem(e.FullName, e.Id.ToString()))
            .ToList();
    }
}
