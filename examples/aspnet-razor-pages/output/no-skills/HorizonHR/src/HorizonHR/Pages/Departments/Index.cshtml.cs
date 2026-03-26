using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Departments;

public class IndexModel : PageModel
{
    private readonly IDepartmentService _departmentService;

    public IndexModel(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    public List<Department> Departments { get; set; } = new();

    public async Task OnGetAsync()
    {
        Departments = await _departmentService.GetTopLevelDepartmentsAsync();
    }
}
