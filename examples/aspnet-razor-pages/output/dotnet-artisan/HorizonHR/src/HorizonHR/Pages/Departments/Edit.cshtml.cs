using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Departments;

public class EditModel(IDepartmentService departmentService, IEmployeeService employeeService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> ParentDepartments { get; set; } = [];
    public List<SelectListItem> Managers { get; set; } = [];

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
        public bool IsActive { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var dept = await departmentService.GetByIdAsync(id);
        if (dept is null) { return NotFound(); }

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

        await LoadSelectLists(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists(Input.Id);
            return Page();
        }

        if (await departmentService.HasCircularReference(Input.Id, Input.ParentDepartmentId))
        {
            ModelState.AddModelError("", "Circular department hierarchy detected.");
            await LoadSelectLists(Input.Id);
            return Page();
        }

        var dept = await departmentService.GetByIdAsync(Input.Id);
        if (dept is null) { return NotFound(); }

        dept.Name = Input.Name;
        dept.Code = Input.Code.ToUpperInvariant();
        dept.Description = Input.Description;
        dept.ParentDepartmentId = Input.ParentDepartmentId;
        dept.ManagerId = Input.ManagerId;
        dept.IsActive = Input.IsActive;

        await departmentService.UpdateAsync(dept);
        TempData["Success"] = $"Department '{dept.Name}' updated successfully.";
        return RedirectToPage("Details", new { id = Input.Id });
    }

    private async Task LoadSelectLists(int departmentId)
    {
        var departments = await departmentService.GetAllActiveAsync();
        ParentDepartments = departments
            .Where(d => d.Id != departmentId)
            .Select(d => new SelectListItem(d.Name, d.Id.ToString()))
            .ToList();

        var employees = await employeeService.GetByDepartmentAsync(departmentId);
        Managers = employees
            .Where(e => e.Status != EmployeeStatus.Terminated)
            .Select(e => new SelectListItem($"{e.FullName} ({e.EmployeeNumber})", e.Id.ToString()))
            .ToList();
    }
}
