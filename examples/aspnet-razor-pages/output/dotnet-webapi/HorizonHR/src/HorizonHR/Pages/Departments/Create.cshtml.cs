using System.ComponentModel.DataAnnotations;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Departments;

public sealed class CreateModel(IDepartmentService departmentService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> DepartmentList { get; set; } = [];

    public sealed class InputModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ParentDepartmentId { get; set; }
    }

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadDepartmentsAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync(ct);
            return Page();
        }

        try
        {
            await departmentService.CreateAsync(Input.Name, Input.Code, Input.Description, Input.ParentDepartmentId, null, ct);
            TempData["SuccessMessage"] = "Department created successfully.";
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadDepartmentsAsync(ct);
            return Page();
        }
    }

    private async Task LoadDepartmentsAsync(CancellationToken ct)
    {
        var depts = await departmentService.GetAllSimpleAsync(ct);
        DepartmentList = depts.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();
    }
}
