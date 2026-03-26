using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;

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

    public IEnumerable<Department> Departments { get; set; } = [];

    public IEnumerable<Employee> Employees { get; set; } = [];

    public class InputModel
    {
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

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
        Input.HireDate = DateOnly.FromDateTime(DateTime.Today);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        try
        {
            var employeeNumber = await _employeeService.GenerateEmployeeNumberAsync();

            var employee = new Employee
            {
                EmployeeNumber = employeeNumber,
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
                ProfileImageUrl = Input.ProfileImageUrl,
                Address = Input.Address,
                City = Input.City,
                State = Input.State,
                ZipCode = Input.ZipCode,
                Status = EmployeeStatus.Active
            };

            var created = await _employeeService.CreateEmployeeAsync(employee);
            TempData["SuccessMessage"] = $"Employee {created.FullName} was created successfully.";
            return RedirectToPage("Details", new { id = created.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadDropdownsAsync();
            return Page();
        }
    }

    private async Task LoadDropdownsAsync()
    {
        Departments = await _departmentService.GetAllDepartmentsAsync();

        var result = await _employeeService.GetEmployeesAsync(null, null, null, null, 1, int.MaxValue);
        Employees = result.Items;
    }
}
