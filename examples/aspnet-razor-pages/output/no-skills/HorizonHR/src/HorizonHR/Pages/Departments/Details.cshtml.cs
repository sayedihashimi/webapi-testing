using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Departments;

public class DetailsModel : PageModel
{
    private readonly IDepartmentService _departmentService;

    public DetailsModel(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    public Department? Department { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Department = await _departmentService.GetByIdAsync(id);
        if (Department == null) return NotFound();
        return Page();
    }
}
