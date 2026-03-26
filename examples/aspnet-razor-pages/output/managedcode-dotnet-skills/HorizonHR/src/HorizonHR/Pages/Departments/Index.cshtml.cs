using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Departments;

public class IndexModel(IDepartmentService departmentService) : PageModel
{
    public List<Department> Departments { get; set; } = [];

    public async Task OnGetAsync()
    {
        Departments = await departmentService.GetAllAsync();
    }
}
