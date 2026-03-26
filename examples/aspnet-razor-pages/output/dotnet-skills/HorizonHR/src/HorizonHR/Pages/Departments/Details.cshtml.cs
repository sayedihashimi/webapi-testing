using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Departments;

public class DetailsModel : PageModel
{
    private readonly IDepartmentService _departmentService;

    public DetailsModel(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Department Department { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        var department = await _departmentService.GetDepartmentByIdAsync(Id);

        if (department == null)
        {
            return NotFound();
        }

        Department = department;
        return Page();
    }
}
