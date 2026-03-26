using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public class EditModel(IEmployeeService employeeService, IDepartmentService departmentService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> Departments { get; set; } = [];
    public List<SelectListItem> Managers { get; set; } = [];

    public class InputModel
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string FirstName { get; set; } = string.Empty;
        [Required, MaxLength(100)] public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress, MaxLength(200)] public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        [Required] public DateOnly DateOfBirth { get; set; }
        [Required] public DateOnly HireDate { get; set; }
        [Required] public int DepartmentId { get; set; }
        [Required, MaxLength(200)] public string JobTitle { get; set; } = string.Empty;
        [Required] public EmploymentType EmploymentType { get; set; }
        [Required, Range(0.01, double.MaxValue)] public decimal Salary { get; set; }
        public int? ManagerId { get; set; }
        [MaxLength(500)] public string? Address { get; set; }
        [MaxLength(100)] public string? City { get; set; }
        [MaxLength(2)] public string? State { get; set; }
        [MaxLength(10)] public string? ZipCode { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var emp = await employeeService.GetByIdAsync(id);
        if (emp is null) { return NotFound(); }
        Input = new InputModel
        {
            Id = emp.Id, FirstName = emp.FirstName, LastName = emp.LastName, Email = emp.Email,
            Phone = emp.Phone, DateOfBirth = emp.DateOfBirth, HireDate = emp.HireDate,
            DepartmentId = emp.DepartmentId, JobTitle = emp.JobTitle, EmploymentType = emp.EmploymentType,
            Salary = emp.Salary, ManagerId = emp.ManagerId,
            Address = emp.Address, City = emp.City, State = emp.State, ZipCode = emp.ZipCode
        };
        await LoadSelectLists();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await LoadSelectLists(); return Page(); }

        var emp = await employeeService.GetByIdAsync(Input.Id);
        if (emp is null) { return NotFound(); }

        emp.FirstName = Input.FirstName; emp.LastName = Input.LastName; emp.Email = Input.Email;
        emp.Phone = Input.Phone; emp.DateOfBirth = Input.DateOfBirth; emp.HireDate = Input.HireDate;
        emp.DepartmentId = Input.DepartmentId; emp.JobTitle = Input.JobTitle;
        emp.EmploymentType = Input.EmploymentType; emp.Salary = Input.Salary; emp.ManagerId = Input.ManagerId;
        emp.Address = Input.Address; emp.City = Input.City; emp.State = Input.State; emp.ZipCode = Input.ZipCode;

        await employeeService.UpdateAsync(emp);
        TempData["Success"] = $"Employee '{emp.FullName}' updated.";
        return RedirectToPage("Details", new { id = Input.Id });
    }

    private async Task LoadSelectLists()
    {
        var depts = await departmentService.GetAllActiveAsync();
        Departments = depts.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();
        var employees = await employeeService.GetAllActiveAsync();
        Managers = employees.Where(e => e.Id != Input.Id).Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();
    }
}
