using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

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

    public List<Employee> Employees { get; set; } = new();
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public EmploymentType? EmploymentTypeFilter { get; set; }
    public EmployeeStatus? StatusFilter { get; set; }
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public List<SelectListItem> DepartmentOptions { get; set; } = new();

    public async Task OnGetAsync(string? search, int? departmentId, EmploymentType? employmentType, EmployeeStatus? status, int pageNumber = 1)
    {
        Search = search;
        DepartmentId = departmentId;
        EmploymentTypeFilter = employmentType;
        StatusFilter = status;
        PageNumber = pageNumber;

        var departments = await _departmentService.GetAllAsync();
        DepartmentOptions = departments.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();

        var (items, totalCount) = await _employeeService.GetPagedAsync(pageNumber, 10, search, departmentId, employmentType, status);
        Employees = items;
        TotalPages = (int)Math.Ceiling(totalCount / 10.0);
    }
}
