using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Reviews;

public class IndexModel(IReviewService reviewService, IDepartmentService departmentService) : PageModel
{
    public PaginatedList<PerformanceReview> Reviews { get; set; } = null!;
    [BindProperty(SupportsGet = true)] public ReviewStatus? Status { get; set; }
    [BindProperty(SupportsGet = true)] public OverallRating? Rating { get; set; }
    [BindProperty(SupportsGet = true)] public int? DepartmentId { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public List<SelectListItem> Departments { get; set; } = [];

    public async Task OnGetAsync()
    {
        var depts = await departmentService.GetAllActiveAsync();
        Departments = depts.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();
        Reviews = await reviewService.GetAllAsync(PageNumber, 10, Status, Rating, DepartmentId);
    }
}
