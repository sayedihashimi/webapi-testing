using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Departments;

public class IndexModel(IDepartmentService departmentService) : PageModel
{
    public List<Department> RootDepartments { get; set; } = [];

    public async Task OnGetAsync()
    {
        RootDepartments = await departmentService.GetHierarchyAsync();
    }
}
