using System.ComponentModel.DataAnnotations;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Departments;

public class EditModel : PageModel
{
    private readonly IDepartmentService _departmentService;
    private readonly IEmployeeService _employeeService;

    public EditModel(IDepartmentService departmentService, IEmployeeService employeeService)
    {
        _departmentService = departmentService;
        _employeeService = employeeService;
    }

    public int Id { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> DepartmentOptions { get; set; } = new();
    public List<SelectListItem> EmployeeOptions { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ParentDepartmentId { get; set; }
        public int? ManagerId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null) return NotFound();

        Id = id;
        Input = new InputModel
        {
            Name = department.Name,
            Code = department.Code,
            Description = department.Description,
            ParentDepartmentId = department.ParentDepartmentId,
            ManagerId = department.ManagerId
        };

        await LoadOptionsAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        Id = id;

        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync(id);
            return Page();
        }

        // Check circular reference
        if (Input.ParentDepartmentId.HasValue &&
            await _departmentService.IsCircularReference(id, Input.ParentDepartmentId))
        {
            ModelState.AddModelError("", "Cannot set parent department: circular reference detected.");
            await LoadOptionsAsync(id);
            return Page();
        }

        var department = await _departmentService.GetByIdAsync(id);
        if (department == null) return NotFound();

        department.Name = Input.Name;
        department.Code = Input.Code;
        department.Description = Input.Description;
        department.ParentDepartmentId = Input.ParentDepartmentId;

        // Validate manager belongs to department
        if (Input.ManagerId.HasValue)
        {
            var manager = department.Employees.FirstOrDefault(e => e.Id == Input.ManagerId.Value);
            if (manager == null)
            {
                ModelState.AddModelError("", "The selected manager must belong to this department.");
                await LoadOptionsAsync(id);
                return Page();
            }
        }
        department.ManagerId = Input.ManagerId;

        await _departmentService.UpdateAsync(department);
        TempData["Success"] = "Department updated successfully.";
        return RedirectToPage("Details", new { id });
    }

    private async Task LoadOptionsAsync(int departmentId)
    {
        var departments = await _departmentService.GetAllAsync();
        DepartmentOptions = departments
            .Where(d => d.Id != departmentId)
            .Select(d => new SelectListItem(d.Name, d.Id.ToString()))
            .ToList();

        var department = await _departmentService.GetByIdAsync(departmentId);
        if (department != null)
        {
            EmployeeOptions = department.Employees
                .Select(e => new SelectListItem(e.FullName, e.Id.ToString()))
                .ToList();
        }
    }
}
