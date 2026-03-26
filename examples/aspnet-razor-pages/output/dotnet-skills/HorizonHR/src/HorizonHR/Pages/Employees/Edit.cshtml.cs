using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;

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

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IEnumerable<Department> Departments { get; set; } = [];

    public IEnumerable<Employee> Employees { get; set; } = [];

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Hire Date")]
        public DateOnly HireDate { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Employment Type")]
        public EmploymentType EmploymentType { get; set; }

        [Required]
        [Range(0.01, 999999999)]
        [Display(Name = "Salary")]
        public decimal Salary { get; set; }

        [Display(Name = "Manager")]
        public int? ManagerId { get; set; }

        [MaxLength(500)]
        [Display(Name = "Profile Image URL")]
        public string? ProfileImageUrl { get; set; }

        [MaxLength(500)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [MaxLength(100)]
        [Display(Name = "City")]
        public string? City { get; set; }

        [MaxLength(2)]
        [Display(Name = "State")]
        public string? State { get; set; }

        [MaxLength(10)]
        [Display(Name = "Zip Code")]
        public string? ZipCode { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);

        if (employee is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            DepartmentId = employee.DepartmentId,
            JobTitle = employee.JobTitle,
            EmploymentType = employee.EmploymentType,
            Salary = employee.Salary,
            ManagerId = employee.ManagerId,
            ProfileImageUrl = employee.ProfileImageUrl,
            Address = employee.Address,
            City = employee.City,
            State = employee.State,
            ZipCode = employee.ZipCode
        };

        await LoadDropdownsAsync(employee.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(Input.Id);
            return Page();
        }

        try
        {
            var existing = await _employeeService.GetEmployeeByIdAsync(Input.Id);

            if (existing is null)
            {
                return NotFound();
            }

            existing.FirstName = Input.FirstName;
            existing.LastName = Input.LastName;
            existing.Email = Input.Email;
            existing.Phone = Input.Phone;
            existing.DateOfBirth = Input.DateOfBirth;
            existing.HireDate = Input.HireDate;
            existing.DepartmentId = Input.DepartmentId;
            existing.JobTitle = Input.JobTitle;
            existing.EmploymentType = Input.EmploymentType;
            existing.Salary = Input.Salary;
            existing.ManagerId = Input.ManagerId;
            existing.ProfileImageUrl = Input.ProfileImageUrl;
            existing.Address = Input.Address;
            existing.City = Input.City;
            existing.State = Input.State;
            existing.ZipCode = Input.ZipCode;

            await _employeeService.UpdateEmployeeAsync(existing);
            TempData["SuccessMessage"] = $"Employee {existing.FullName} was updated successfully.";
            return RedirectToPage("Details", new { id = existing.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadDropdownsAsync(Input.Id);
            return Page();
        }
    }

    private async Task LoadDropdownsAsync(int currentEmployeeId)
    {
        Departments = await _departmentService.GetAllDepartmentsAsync();

        var result = await _employeeService.GetEmployeesAsync(null, null, null, null, 1, int.MaxValue);
        Employees = result.Items.Where(e => e.Id != currentEmployeeId);
    }
}
