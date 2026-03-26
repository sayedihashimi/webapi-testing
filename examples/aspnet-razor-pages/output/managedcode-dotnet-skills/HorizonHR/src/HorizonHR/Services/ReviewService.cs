using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class ReviewService(ApplicationDbContext db, ILogger<ReviewService> logger) : IReviewService
{
    public async Task<(List<PerformanceReview> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize,
        ReviewStatus? status = null, OverallRating? rating = null,
        int? departmentId = null)
    {
        var query = db.PerformanceReviews
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Reviewer)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue) query = query.Where(r => r.Status == status.Value);
        if (rating.HasValue) query = query.Where(r => r.OverallRating == rating.Value);
        if (departmentId.HasValue) query = query.Where(r => r.Employee.DepartmentId == departmentId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.ReviewPeriodEnd)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<PerformanceReview?> GetByIdAsync(int id)
    {
        return await db.PerformanceReviews
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Reviewer)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<PerformanceReview> CreateAsync(PerformanceReview review)
    {
        if (review.ReviewPeriodEnd <= review.ReviewPeriodStart)
            throw new InvalidOperationException("Review period end must be after start.");

        // Check for overlapping review periods
        var hasOverlap = await db.PerformanceReviews
            .AnyAsync(r => r.EmployeeId == review.EmployeeId
                && r.ReviewPeriodStart <= review.ReviewPeriodEnd
                && r.ReviewPeriodEnd >= review.ReviewPeriodStart);

        if (hasOverlap)
            throw new InvalidOperationException("An overlapping performance review already exists for this employee.");

        review.Status = ReviewStatus.Draft;
        db.PerformanceReviews.Add(review);
        await db.SaveChangesAsync();
        logger.LogInformation("Performance review created for employee {EmpId}", review.EmployeeId);
        return review;
    }

    public async Task SubmitSelfAssessmentAsync(int reviewId, string selfAssessment)
    {
        var review = await db.PerformanceReviews.FindAsync(reviewId)
            ?? throw new InvalidOperationException("Review not found.");

        if (review.Status != ReviewStatus.SelfAssessmentPending)
            throw new InvalidOperationException("Review is not in self-assessment pending status.");

        review.SelfAssessment = selfAssessment;
        review.Status = ReviewStatus.ManagerReviewPending;
        await db.SaveChangesAsync();
    }

    public async Task CompleteManagerReviewAsync(int reviewId, string managerAssessment,
        OverallRating rating, string? strengths, string? areasForImprovement, string? goals)
    {
        var review = await db.PerformanceReviews.FindAsync(reviewId)
            ?? throw new InvalidOperationException("Review not found.");

        if (review.Status != ReviewStatus.ManagerReviewPending)
            throw new InvalidOperationException("Review is not in manager review pending status.");

        review.ManagerAssessment = managerAssessment;
        review.OverallRating = rating;
        review.StrengthsNoted = strengths;
        review.AreasForImprovement = areasForImprovement;
        review.Goals = goals;
        review.Status = ReviewStatus.Completed;
        review.CompletedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        await db.SaveChangesAsync();
        logger.LogInformation("Performance review {Id} completed with rating {Rating}", reviewId, rating);
    }
}
