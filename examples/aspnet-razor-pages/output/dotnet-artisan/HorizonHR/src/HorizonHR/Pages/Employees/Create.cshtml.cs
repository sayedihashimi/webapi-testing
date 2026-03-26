using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public class CreateModel(IEmployeeService employeeService, IDepartmentService departmentService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> Departments { get; set; } = [];
    public List<SelectListItem> Managers { get; set; } = [];

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

        [Required, MaxLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public EmploymentType EmploymentType { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Salary must be positive")]
        public decimal Salary { get; set; }

        public int? ManagerId { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(2)]
        public string? State { get; set; }

        [MaxLength(10)]
        public string? ZipCode { get; set; }
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

        var employee = new Employee
        {
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            Email = Input.Email,
            Phone = Input.Phone,
            DateOfBirth = Input.DateOfBirth,
            HireDate = Input.HireDate,
            DepartmentId = Input.DepartmentId,
            JobTitle = Input.JobTitle,
            EmploymentType = Input.EmploymentType,
            Salary = Input.Salary,
            ManagerId = Input.ManagerId,
            Address = Input.Address,
            City = Input.City,
            State = Input.State,
            ZipCode = Input.ZipCode,
            Status = EmployeeStatus.Active
        };

        await employeeService.CreateAsync(employee);
        TempData["Success"] = $"Employee '{employee.FullName}' created with number {employee.EmployeeNumber}.";
        return RedirectToPage("Details", new { id = employee.Id });
    }

    private async Task LoadSelectLists()
    {
        var depts = await departmentService.GetAllActiveAsync();
        Departments = depts.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();

        var employees = await employeeService.GetAllActiveAsync();
        Managers = employees.Select(e => new SelectListItem($"{e.FullName}", e.Id.ToString())).ToList();
    }
}
