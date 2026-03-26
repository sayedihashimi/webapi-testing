using System.ComponentModel.DataAnnotations;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public class EditModel : PageModel
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;

    public EditModel(IEmployeeService employeeService, IDepartmentService departmentService)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
    }

    public int Id { get; set; }

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
        public int DepartmentId { get; set; }

        public int? ManagerId { get; set; }

        [Required, MaxLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public EmploymentType EmploymentType { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Salary { get; set; }

        public EmployeeStatus Status { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(2)]
        public string? State { get; set; }

        [MaxLength(10)]
        public string? ZipCode { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null) return NotFound();

        Id = id;
        Input = new InputModel
        {
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            DepartmentId = employee.DepartmentId,
            ManagerId = employee.ManagerId,
            JobTitle = employee.JobTitle,
            EmploymentType = employee.EmploymentType,
            Salary = employee.Salary,
            Status = employee.Status,
            Address = employee.Address,
            City = employee.City,
            State = employee.State,
            ZipCode = employee.ZipCode
        };

        await LoadOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        Id = id;

        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync();
            return Page();
        }

        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null) return NotFound();

        var oldDepartmentId = employee.DepartmentId;

        employee.FirstName = Input.FirstName;
        employee.LastName = Input.LastName;
        employee.Email = Input.Email;
        employee.Phone = Input.Phone;
        employee.DepartmentId = Input.DepartmentId;
        employee.JobTitle = Input.JobTitle;
        employee.EmploymentType = Input.EmploymentType;
        employee.Salary = Input.Salary;
        employee.Status = Input.Status;
        employee.Address = Input.Address;
        employee.City = Input.City;
        employee.State = Input.State;
        employee.ZipCode = Input.ZipCode;

        // Clear manager if department changed and old manager is not in new department
        if (oldDepartmentId != Input.DepartmentId && Input.ManagerId.HasValue)
        {
            var manager = await _employeeService.GetByIdAsync(Input.ManagerId.Value);
            if (manager == null || manager.DepartmentId != Input.DepartmentId)
                employee.ManagerId = null;
            else
                employee.ManagerId = Input.ManagerId;
        }
        else
        {
            employee.ManagerId = Input.ManagerId;
        }

        await _employeeService.UpdateAsync(employee);
        TempData["Success"] = "Employee updated successfully.";
        return RedirectToPage("Details", new { id });
    }

    private async Task LoadOptionsAsync()
    {
        var departments = await _departmentService.GetAllAsync();
        DepartmentOptions = departments.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();

        var employees = await _employeeService.GetAllActiveAsync();
        ManagerOptions = employees.Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();
    }
}
