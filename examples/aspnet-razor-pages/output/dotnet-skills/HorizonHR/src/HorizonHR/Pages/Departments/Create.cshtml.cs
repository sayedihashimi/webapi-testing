using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Departments;

public class CreateModel : PageModel
{
    private readonly IDepartmentService _departmentService;
    private readonly IEmployeeService _employeeService;

    public CreateModel(IDepartmentService departmentService, IEmployeeService employeeService)
    {
        _departmentService = departmentService;
        _employeeService = employeeService;
    }

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
    }

    public async Task OnGetAsync()
    {
        await PopulateDropdownsAsync();
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
            var department = new Department
            {
                Name = Input.Name,
                Code = Input.Code,
                Description = Input.Description,
                ParentDepartmentId = Input.ParentDepartmentId,
                ManagerId = Input.ManagerId
            };

            await _departmentService.CreateDepartmentAsync(department);

            TempData["SuccessMessage"] = $"Department '{department.Name}' was created successfully.";
            return RedirectToPage("Index");
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
        ParentDepartments = new SelectList(departments, nameof(Department.Id), nameof(Department.Name));

        var (employees, _) = await _employeeService.GetEmployeesAsync(
            searchTerm: null, departmentId: null, employmentType: null, status: null, page: 1, pageSize: int.MaxValue);
        Managers = new SelectList(employees, nameof(Employee.Id), nameof(Employee.FullName));
    }
}
