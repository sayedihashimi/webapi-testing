using System.ComponentModel.DataAnnotations;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public sealed class EditModel(IEmployeeService employeeService, IDepartmentService departmentService) : PageModel
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> DepartmentList { get; set; } = [];
    public List<SelectListItem> ManagerList { get; set; } = [];

    public sealed class InputModel
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public DateOnly HireDate { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required, MaxLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        public EmploymentType EmploymentType { get; set; }

        [Range(0.01, 9999999.99)]
        public decimal Salary { get; set; }

        public int? ManagerId { get; set; }
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

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var emp = await employeeService.GetByIdAsync(id, ct);
        if (emp == null) return NotFound();

        Id = id;
        EmployeeName = $"{emp.FirstName} {emp.LastName}";
        Input = new InputModel
        {
            FirstName = emp.FirstName,
            LastName = emp.LastName,
            Email = emp.Email,
            Phone = emp.Phone,
            DateOfBirth = emp.DateOfBirth,
            HireDate = emp.HireDate,
            DepartmentId = emp.DepartmentId,
            JobTitle = emp.JobTitle,
            EmploymentType = emp.EmploymentType,
            Salary = emp.Salary,
            ManagerId = emp.ManagerId,
            Status = emp.Status,
            Address = emp.Address,
            City = emp.City,
            State = emp.State,
            ZipCode = emp.ZipCode
        };

        await LoadDropdownsAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        Id = id;
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(ct);
            return Page();
        }

        try
        {
            await employeeService.UpdateAsync(id, Input.FirstName, Input.LastName, Input.Email,
                Input.Phone, Input.DateOfBirth, Input.HireDate, Input.DepartmentId,
                Input.JobTitle, Input.EmploymentType, Input.Salary, Input.ManagerId,
                Input.Status, null, Input.Address, Input.City, Input.State, Input.ZipCode, ct);

            TempData["SuccessMessage"] = "Employee updated successfully.";
            return RedirectToPage("Details", new { id });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadDropdownsAsync(ct);
            return Page();
        }
    }

    private async Task LoadDropdownsAsync(CancellationToken ct)
    {
        var depts = await departmentService.GetAllSimpleAsync(ct);
        DepartmentList = depts.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();

        var allEmployees = await employeeService.GetAllAsync(null, null, null, EmployeeStatus.Active, 1, 1000, ct);
        ManagerList = allEmployees.Items.Where(e => e.Id != Id).Select(e => new SelectListItem($"{e.FirstName} {e.LastName}", e.Id.ToString())).ToList();
    }
}
