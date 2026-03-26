using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Employees;

public sealed class IndexModel(IEmployeeService employeeService, IDepartmentService departmentService) : PageModel
{
    public PaginatedList<Employee> Employees { get; set; } = null!;
    public List<Department> Departments { get; set; } = [];
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public EmployeeStatus? Status { get; set; }

    public async Task OnGetAsync(string? search, int? departmentId, EmploymentType? employmentType, EmployeeStatus? status, int pageNumber = 1, CancellationToken ct = default)
    {
        Search = search;
        DepartmentId = departmentId;
        EmploymentType = employmentType;
        Status = status;

        Departments = await departmentService.GetAllSimpleAsync(ct);
        Employees = await employeeService.GetAllAsync(search, departmentId, employmentType, status, pageNumber, 10, ct);
    }
}
