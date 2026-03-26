using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public class IndexModel(IEmployeeService employeeService, IDepartmentService departmentService) : PageModel
{
    public List<Employee> Employees { get; set; } = [];
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public string? SearchTerm { get; set; }
    public int? DepartmentId { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public EmployeeStatus? Status { get; set; }
    public List<SelectListItem> DepartmentOptions { get; set; } = [];

    public async Task OnGetAsync(int pageNumber = 1, int pageSize = 10,
        string? searchTerm = null, int? departmentId = null,
        EmploymentType? employmentType = null, EmployeeStatus? status = null)
    {
        PageNumber = pageNumber;
        SearchTerm = searchTerm;
        DepartmentId = departmentId;
        EmploymentType = employmentType;
        Status = status;

        var (items, totalCount) = await employeeService.GetPagedAsync(
            pageNumber, pageSize, searchTerm, departmentId, employmentType, status);

        Employees = items;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var departments = await departmentService.GetAllAsync();
        DepartmentOptions = departments.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();
    }
}
