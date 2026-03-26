using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Departments;

public sealed class IndexModel(IDepartmentService departmentService) : PageModel
{
    public List<Department> Departments { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Departments = await departmentService.GetHierarchyAsync(ct);
    }
}
