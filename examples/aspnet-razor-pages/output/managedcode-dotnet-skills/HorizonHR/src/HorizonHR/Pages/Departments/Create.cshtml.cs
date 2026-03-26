using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Departments;

public class CreateModel(IDepartmentService departmentService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> DepartmentOptions { get; set; } = [];

    public class InputModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ParentDepartmentId { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync();
            return Page();
        }

        try
        {
            var department = new Department
            {
                Name = Input.Name,
                Code = Input.Code.ToUpperInvariant(),
                Description = Input.Description,
                ParentDepartmentId = Input.ParentDepartmentId
            };

            await departmentService.CreateAsync(department);
            TempData["SuccessMessage"] = $"Department '{department.Name}' created successfully.";
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadOptionsAsync();
            return Page();
        }
    }

    private async Task LoadOptionsAsync()
    {
        var departments = await departmentService.GetAllAsync();
        DepartmentOptions = departments.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();
    }
}
