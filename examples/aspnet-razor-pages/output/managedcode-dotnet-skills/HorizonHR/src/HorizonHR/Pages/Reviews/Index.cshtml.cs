using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Reviews;

public class IndexModel(IReviewService reviewService, IDepartmentService departmentService) : PageModel
{
    public List<PerformanceReview> Reviews { get; set; } = [];
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public ReviewStatus? StatusFilter { get; set; }
    public int? DepartmentId { get; set; }
    public List<SelectListItem> DepartmentOptions { get; set; } = [];

    public async Task OnGetAsync(int pageNumber = 1, int pageSize = 10,
        ReviewStatus? status = null, int? departmentId = null)
    {
        PageNumber = pageNumber;
        StatusFilter = status;
        DepartmentId = departmentId;

        var (items, total) = await reviewService.GetPagedAsync(pageNumber, pageSize, status, departmentId: departmentId);
        Reviews = items;
        TotalPages = (int)Math.Ceiling(total / (double)pageSize);

        var depts = await departmentService.GetAllAsync();
        DepartmentOptions = depts.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();
    }
}
