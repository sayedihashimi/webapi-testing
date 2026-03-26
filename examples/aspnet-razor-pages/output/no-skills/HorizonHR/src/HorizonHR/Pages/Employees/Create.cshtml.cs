using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public class CreateModel : PageModel
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;

    public CreateModel(IEmployeeService employeeService, IDepartmentService departmentService)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> DepartmentOptions { get; set; } = new();
    public List<SelectListItem> ManagerOptions { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public DateOnly HireDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        public int DepartmentId { get; set; }

        public int? ManagerId { get; set; }

        [Required, MaxLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public EmploymentType EmploymentType { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Salary must be positive")]
        public decimal Salary { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync();
            return Page();
        }

        var employee = new Employee
        {
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            Email = Input.Email,
            Phone = Input.Phone,
            DateOfBirth = Input.DateOfBirth,
            HireDate = Input.HireDate,
            DepartmentId = Input.DepartmentId,
            ManagerId = Input.ManagerId,
            JobTitle = Input.JobTitle,
            EmploymentType = Input.EmploymentType,
            Salary = Input.Salary
        };

        await _employeeService.CreateAsync(employee);
        TempData["Success"] = $"Employee {employee.FullName} ({employee.EmployeeNumber}) created successfully.";
        return RedirectToPage("Details", new { id = employee.Id });
    }

    private async Task LoadOptionsAsync()
    {
        var departments = await _departmentService.GetAllAsync();
        DepartmentOptions = departments.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();

        var employees = await _employeeService.GetAllActiveAsync();
        ManagerOptions = employees.Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();
    }
}
