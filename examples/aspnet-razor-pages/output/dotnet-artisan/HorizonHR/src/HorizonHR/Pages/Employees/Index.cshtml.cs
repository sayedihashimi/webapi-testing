using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public class IndexModel(IEmployeeService employeeService, IDepartmentService departmentService) : PageModel
{
    public PaginatedList<Employee> Employees { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DepartmentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public EmploymentType? EmploymentType { get; set; }

    [BindProperty(SupportsGet = true)]
    public EmployeeStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public List<SelectListItem> Departments { get; set; } = [];

    public async Task OnGetAsync()
    {
        var depts = await departmentService.GetAllActiveAsync();
        Departments = depts.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();
        Employees = await employeeService.GetAllAsync(PageNumber, 10, Search, DepartmentId, EmploymentType, Status);
    }
}
