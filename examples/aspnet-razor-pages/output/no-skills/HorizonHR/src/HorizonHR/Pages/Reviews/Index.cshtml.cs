using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Reviews;

public class IndexModel : PageModel
{
    private readonly IReviewService _reviewService;
    private readonly IDepartmentService _departmentService;

    public IndexModel(IReviewService reviewService, IDepartmentService departmentService)
    {
        _reviewService = reviewService;
        _departmentService = departmentService;
    }

    public List<PerformanceReview> Reviews { get; set; } = new();
    public ReviewStatus? StatusFilter { get; set; }
    public OverallRating? RatingFilter { get; set; }
    public int? DepartmentFilter { get; set; }
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public List<SelectListItem> DepartmentOptions { get; set; } = new();

    public async Task OnGetAsync(ReviewStatus? status, OverallRating? rating, int? departmentId, int pageNumber = 1)
    {
        StatusFilter = status;
        RatingFilter = rating;
        DepartmentFilter = departmentId;
        PageNumber = pageNumber;

        var departments = await _departmentService.GetAllAsync();
        DepartmentOptions = departments.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();

        var (items, totalCount) = await _reviewService.GetPagedAsync(pageNumber, 10, status, rating, departmentId);
        Reviews = items;
        TotalPages = (int)Math.Ceiling(totalCount / 10.0);
    }
}
