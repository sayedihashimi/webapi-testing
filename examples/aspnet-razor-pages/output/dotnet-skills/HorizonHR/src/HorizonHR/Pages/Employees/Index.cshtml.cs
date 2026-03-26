using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Employees;

public class IndexModel : PageModel
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;

    public IndexModel(IEmployeeService employeeService, IDepartmentService departmentService)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DepartmentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public EmploymentType? EmploymentType { get; set; }

    [BindProperty(SupportsGet = true)]
    public EmployeeStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public IEnumerable<Employee> Employees { get; set; } = [];

    public int TotalCount { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public IEnumerable<Department> Departments { get; set; } = [];

    public async Task OnGetAsync()
    {
        Departments = await _departmentService.GetAllDepartmentsAsync();

        var result = await _employeeService.GetEmployeesAsync(
            SearchTerm, DepartmentId, EmploymentType, Status, PageNumber, PageSize);

        Employees = result.Items;
        TotalCount = result.TotalCount;
    }
}
