using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(ApplicationDbContext context, ILogger<ReviewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<PerformanceReview> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, ReviewStatus? status = null, OverallRating? rating = null, int? departmentId = null)
    {
        var query = _context.PerformanceReviews
            .Include(pr => pr.Employee).ThenInclude(e => e.Department)
            .Include(pr => pr.Reviewer)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(pr => pr.Status == status.Value);
        if (rating.HasValue)
            query = query.Where(pr => pr.OverallRating == rating.Value);
        if (departmentId.HasValue)
            query = query.Where(pr => pr.Employee.DepartmentId == departmentId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(pr => pr.ReviewPeriodEnd)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<PerformanceReview?> GetByIdAsync(int id)
    {
        return await _context.PerformanceReviews
            .Include(pr => pr.Employee).ThenInclude(e => e.Department)
            .Include(pr => pr.Reviewer)
            .FirstOrDefaultAsync(pr => pr.Id == id);
    }

    public async Task<PerformanceReview> CreateAsync(PerformanceReview review)
    {
        // Check for overlapping review periods
        var hasOverlap = await _context.PerformanceReviews
            .AnyAsync(pr => pr.EmployeeId == review.EmployeeId
                && pr.ReviewPeriodStart <= review.ReviewPeriodEnd
                && pr.ReviewPeriodEnd >= review.ReviewPeriodStart);

        if (hasOverlap)
            throw new InvalidOperationException("This employee already has a performance review with an overlapping period.");

        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Performance review created for employee {EmployeeId}", review.EmployeeId);
        return review;
    }

    public async Task SubmitSelfAssessmentAsync(int id, string selfAssessment)
    {
        var review = await _context.PerformanceReviews.FindAsync(id);
        if (review == null) throw new InvalidOperationException("Review not found.");
        if (review.Status != ReviewStatus.SelfAssessmentPending)
            throw new InvalidOperationException("Self-assessment can only be submitted when status is SelfAssessmentPending.");

        review.SelfAssessment = selfAssessment;
        review.Status = ReviewStatus.ManagerReviewPending;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Self-assessment submitted for review {Id}", id);
    }

    public async Task CompleteManagerReviewAsync(int id, string managerAssessment, OverallRating rating,
        string? strengths, string? areasForImprovement, string? goals)
    {
        var review = await _context.PerformanceReviews.FindAsync(id);
        if (review == null) throw new InvalidOperationException("Review not found.");
        if (review.Status != ReviewStatus.ManagerReviewPending)
            throw new InvalidOperationException("Manager review can only be completed when status is ManagerReviewPending.");

        review.ManagerAssessment = managerAssessment;
        review.OverallRating = rating;
        review.StrengthsNoted = strengths;
        review.AreasForImprovement = areasForImprovement;
        review.Goals = goals;
        review.CompletedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        review.Status = ReviewStatus.Completed;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Performance review {Id} completed with rating {Rating}", id, rating);
    }

    public async Task<int> GetUpcomingCountAsync()
    {
        return await _context.PerformanceReviews
            .CountAsync(pr => pr.Status != ReviewStatus.Completed);
    }
}
