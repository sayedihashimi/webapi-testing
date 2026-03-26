using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;

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
    public List<Department> Departments { get; set; } = new();
    public PaginationModel Pagination { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public ReviewStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public OverallRating? Rating { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DepartmentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public async Task OnGetAsync()
    {
        Departments = await _departmentService.GetAllDepartmentsAsync();

        var (items, totalCount) = await _reviewService.GetReviewsAsync(
            Status, Rating, DepartmentId, PageNumber, PageSize);

        Reviews = items;
        Pagination = new PaginationModel
        {
            CurrentPage = PageNumber,
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize),
            PageUrl = "/Reviews/Index"
        };
    }
}
