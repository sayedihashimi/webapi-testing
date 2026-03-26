using System.ComponentModel.DataAnnotations;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public sealed class CreateModel(IEmployeeService employeeService, IDepartmentService departmentService) : PageModel
{
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
        public DateOnly HireDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        public int DepartmentId { get; set; }

        [Required, MaxLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;

        [Range(0.01, 9999999.99)]
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

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadDropdownsAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(ct);
            return Page();
        }

        try
        {
            var emp = await employeeService.CreateAsync(
                Input.FirstName, Input.LastName, Input.Email, Input.Phone,
                Input.DateOfBirth, Input.HireDate, Input.DepartmentId,
                Input.JobTitle, Input.EmploymentType, Input.Salary,
                Input.ManagerId, null, Input.Address, Input.City,
                Input.State, Input.ZipCode, ct);

            TempData["SuccessMessage"] = $"Employee {emp.EmployeeNumber} created successfully.";
            return RedirectToPage("Details", new { id = emp.Id });
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
        // Manager list: all employees
        var allEmployees = await employeeService.GetAllAsync(null, null, null, EmployeeStatus.Active, 1, 1000, ct);
        ManagerList = allEmployees.Items.Select(e => new SelectListItem($"{e.FirstName} {e.LastName} ({e.EmployeeNumber})", e.Id.ToString())).ToList();
    }
}
