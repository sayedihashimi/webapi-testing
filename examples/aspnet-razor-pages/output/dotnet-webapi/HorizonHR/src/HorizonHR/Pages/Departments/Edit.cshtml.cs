using System.ComponentModel.DataAnnotations;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Departments;

public sealed class EditModel(IDepartmentService departmentService, IEmployeeService employeeService) : PageModel
{
    public int Id { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> DepartmentList { get; set; } = [];
    public List<SelectListItem> EmployeeList { get; set; } = [];

    public sealed class InputModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ParentDepartmentId { get; set; }
        public int? ManagerId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var dept = await departmentService.GetByIdAsync(id, ct);
        if (dept == null) return NotFound();

        Id = id;
        Input = new InputModel
        {
            Name = dept.Name,
            Code = dept.Code,
            Description = dept.Description,
            ParentDepartmentId = dept.ParentDepartmentId,
            ManagerId = dept.ManagerId,
            IsActive = dept.IsActive
        };

        await LoadDropdownsAsync(id, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        Id = id;
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(id, ct);
            return Page();
        }

        try
        {
            await departmentService.UpdateAsync(id, Input.Name, Input.Code, Input.Description, Input.ParentDepartmentId, Input.ManagerId, Input.IsActive, ct);
            TempData["SuccessMessage"] = "Department updated successfully.";
            return RedirectToPage("Details", new { id });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadDropdownsAsync(id, ct);
            return Page();
        }
    }

    private async Task LoadDropdownsAsync(int deptId, CancellationToken ct)
    {
        var depts = await departmentService.GetAllSimpleAsync(ct);
        DepartmentList = depts.Where(d => d.Id != deptId).Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();

        var employees = await employeeService.GetByDepartmentAsync(deptId, ct);
        EmployeeList = employees.Select(e => new SelectListItem($"{e.FirstName} {e.LastName}", e.Id.ToString())).ToList();
    }
}
