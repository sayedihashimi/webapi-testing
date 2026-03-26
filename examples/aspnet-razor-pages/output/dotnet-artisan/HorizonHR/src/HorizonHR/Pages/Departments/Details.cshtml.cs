using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Departments;

public class DetailsModel(IDepartmentService departmentService) : PageModel
{
    public Department Department { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var dept = await departmentService.GetByIdAsync(id);
        if (dept is null) { return NotFound(); }
        Department = dept;
        return Page();
    }
}
