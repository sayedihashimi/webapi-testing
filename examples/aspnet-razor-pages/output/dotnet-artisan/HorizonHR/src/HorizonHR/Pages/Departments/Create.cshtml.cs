using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Departments;

public class CreateModel(IDepartmentService departmentService, IEmployeeService employeeService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> ParentDepartments { get; set; } = [];
    public List<SelectListItem> Managers { get; set; } = [];

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

    public async Task OnGetAsync()
    {
        await LoadSelectLists();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return Page();
        }

        var department = new Department
        {
            Name = Input.Name,
            Code = Input.Code.ToUpperInvariant(),
            Description = Input.Description,
            ParentDepartmentId = Input.ParentDepartmentId,
            ManagerId = Input.ManagerId
        };

        if (Input.ParentDepartmentId.HasValue)
        {
            var hasCircular = await departmentService.HasCircularReference(0, Input.ParentDepartmentId);
            if (hasCircular)
            {
                ModelState.AddModelError("", "Circular department hierarchy detected.");
                await LoadSelectLists();
                return Page();
            }
        }

        await departmentService.CreateAsync(department);
        TempData["Success"] = $"Department '{department.Name}' created successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadSelectLists()
    {
        var departments = await departmentService.GetAllActiveAsync();
        ParentDepartments = departments.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();

        var employees = await employeeService.GetAllActiveAsync();
        Managers = employees.Select(e => new SelectListItem($"{e.FullName} ({e.EmployeeNumber})", e.Id.ToString())).ToList();
    }
}
