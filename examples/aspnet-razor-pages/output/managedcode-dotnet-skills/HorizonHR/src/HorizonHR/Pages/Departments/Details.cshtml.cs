using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Departments;

public class DetailsModel(IDepartmentService departmentService) : PageModel
{
    public Department? Department { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Department = await departmentService.GetByIdAsync(id);
        if (Department is null) return NotFound();
        return Page();
    }
}
