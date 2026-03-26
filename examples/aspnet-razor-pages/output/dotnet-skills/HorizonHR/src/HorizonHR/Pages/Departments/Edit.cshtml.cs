using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;

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

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ParentDepartments { get; set; } = null!;
    public SelectList Managers { get; set; } = null!;

    public class InputModel
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "Department Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        [Display(Name = "Department Code")]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Parent Department")]
        public int? ParentDepartmentId { get; set; }

        [Display(Name = "Manager")]
        public int? ManagerId { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var department = await _departmentService.GetDepartmentByIdAsync(Id);

        if (department == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Name = department.Name,
            Code = department.Code,
            Description = department.Description,
            ParentDepartmentId = department.ParentDepartmentId,
            ManagerId = department.ManagerId,
            IsActive = department.IsActive
        };

        await PopulateDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return Page();
        }

        try
        {
            var department = await _departmentService.GetDepartmentByIdAsync(Id);

            if (department == null)
            {
                return NotFound();
            }

            department.Name = Input.Name;
            department.Code = Input.Code;
            department.Description = Input.Description;
            department.ParentDepartmentId = Input.ParentDepartmentId;
            department.ManagerId = Input.ManagerId;
            department.IsActive = Input.IsActive;

            await _departmentService.UpdateDepartmentAsync(department);

            TempData["SuccessMessage"] = $"Department '{department.Name}' was updated successfully.";
            return RedirectToPage("Details", new { id = Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await PopulateDropdownsAsync();
            return Page();
        }
    }

    private async Task PopulateDropdownsAsync()
    {
        var departments = await _departmentService.GetAllDepartmentsAsync();
        // Exclude self from parent dropdown to prevent self-referencing
        var filteredDepartments = departments.Where(d => d.Id != Id).ToList();
        ParentDepartments = new SelectList(filteredDepartments, nameof(Department.Id), nameof(Department.Name));

        // Filter managers to employees in this department
        var departmentEmployees = await _employeeService.GetEmployeesByDepartmentAsync(Id);
        Managers = new SelectList(departmentEmployees, nameof(Employee.Id), nameof(Employee.FullName));
    }
}
