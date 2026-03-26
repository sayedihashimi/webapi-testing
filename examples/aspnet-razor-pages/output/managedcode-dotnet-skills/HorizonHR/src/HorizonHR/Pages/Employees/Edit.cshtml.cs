using System.ComponentModel.DataAnnotations;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public class EditModel(IEmployeeService employeeService, IDepartmentService departmentService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> DepartmentOptions { get; set; } = [];
    public List<SelectListItem> ManagerOptions { get; set; } = [];

    public class InputModel
    {
        public int Id { get; set; }
        [Required, MaxLength(100), Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        [Required, MaxLength(100), Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        [Required, Display(Name = "Department")]
        public int DepartmentId { get; set; }
        [Required, MaxLength(200), Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty;
        [Display(Name = "Employment Type")]
        public EmploymentType EmploymentType { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal Salary { get; set; }
        public int? ManagerId { get; set; }
        [MaxLength(500)]
        public string? Address { get; set; }
        [MaxLength(100)]
        public string? City { get; set; }
        [MaxLength(2)]
        public string? State { get; set; }
        [MaxLength(10), Display(Name = "Zip Code")]
        public string? ZipCode { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var emp = await employeeService.GetByIdAsync(id);
        if (emp is null) return NotFound();

        Input = new InputModel
        {
            Id = emp.Id,
            FirstName = emp.FirstName,
            LastName = emp.LastName,
            Email = emp.Email,
            Phone = emp.Phone,
            DepartmentId = emp.DepartmentId,
            JobTitle = emp.JobTitle,
            EmploymentType = emp.EmploymentType,
            Salary = emp.Salary,
            ManagerId = emp.ManagerId,
            Address = emp.Address,
            City = emp.City,
            State = emp.State,
            ZipCode = emp.ZipCode
        };

        await LoadOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync();
            return Page();
        }

        var emp = await employeeService.GetByIdAsync(Input.Id);
        if (emp is null) return NotFound();

        emp.FirstName = Input.FirstName;
        emp.LastName = Input.LastName;
        emp.Email = Input.Email;
        emp.Phone = Input.Phone;
        emp.DepartmentId = Input.DepartmentId;
        emp.JobTitle = Input.JobTitle;
        emp.EmploymentType = Input.EmploymentType;
        emp.Salary = Input.Salary;
        emp.ManagerId = Input.ManagerId;
        emp.Address = Input.Address;
        emp.City = Input.City;
        emp.State = Input.State;
        emp.ZipCode = Input.ZipCode;

        await employeeService.UpdateAsync(emp);
        TempData["SuccessMessage"] = $"Employee '{emp.FullName}' updated.";
        return RedirectToPage("Details", new { id = emp.Id });
    }

    private async Task LoadOptionsAsync()
    {
        var departments = await departmentService.GetAllAsync();
        DepartmentOptions = departments.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();

        var (employees, _) = await employeeService.GetPagedAsync(1, 500, status: EmployeeStatus.Active);
        ManagerOptions = employees
            .Where(e => e.Id != Input.Id)
            .Select(e => new SelectListItem($"{e.FullName} ({e.Department.Name})", e.Id.ToString()))
            .ToList();
    }
}
