using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Reviews;

public sealed class IndexModel(IReviewService reviewService) : PageModel
{
    public PaginatedList<PerformanceReview> Reviews { get; set; } = null!;
    public ReviewStatus? Status { get; set; }

    public async Task OnGetAsync(ReviewStatus? status, int pageNumber = 1, CancellationToken ct = default)
    {
        Status = status;
        Reviews = await reviewService.GetAllAsync(status, null, null, pageNumber, 10, ct);
    }
}
