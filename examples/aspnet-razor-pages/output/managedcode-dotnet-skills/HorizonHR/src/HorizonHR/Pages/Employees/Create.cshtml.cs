using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public class CreateModel(IEmployeeService employeeService, IDepartmentService departmentService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> DepartmentOptions { get; set; } = [];
    public List<SelectListItem> ManagerOptions { get; set; } = [];

    public class InputModel
    {
        [Required, MaxLength(100), Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100), Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Required, Display(Name = "Date of Birth")]
        public DateOnly DateOfBirth { get; set; }

        [Required, Display(Name = "Hire Date")]
        public DateOnly HireDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required, Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required, MaxLength(200), Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty;

        [Display(Name = "Employment Type")]
        public EmploymentType EmploymentType { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be positive")]
        public decimal Salary { get; set; }

        public int? ManagerId { get; set; }
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

        try
        {
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
                Status = EmployeeStatus.Active
            };

            await employeeService.CreateAsync(employee);
            TempData["SuccessMessage"] = $"Employee '{employee.FullName}' ({employee.EmployeeNumber}) created successfully.";
            return RedirectToPage("Details", new { id = employee.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadOptionsAsync();
            return Page();
        }
    }

    private async Task LoadOptionsAsync()
    {
        var departments = await departmentService.GetAllAsync();
        DepartmentOptions = departments.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();

        var (employees, _) = await employeeService.GetPagedAsync(1, 500, status: EmployeeStatus.Active);
        ManagerOptions = employees.Select(e => new SelectListItem($"{e.FullName} ({e.Department.Name})", e.Id.ToString())).ToList();
    }
}
