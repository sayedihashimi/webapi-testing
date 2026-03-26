using HorizonHR.Data;
using HorizonHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public sealed class ReviewService(ApplicationDbContext db, ILogger<ReviewService> logger) : IReviewService
{
    public async Task<PaginatedList<PerformanceReview>> GetAllAsync(int pageNumber, int pageSize,
        ReviewStatus? status = null, OverallRating? rating = null, int? departmentId = null,
        CancellationToken ct = default)
    {
        var query = db.PerformanceReviews
            .AsNoTracking()
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Reviewer)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }
        if (rating.HasValue)
        {
            query = query.Where(r => r.OverallRating == rating.Value);
        }
        if (departmentId.HasValue)
        {
            query = query.Where(r => r.Employee.DepartmentId == departmentId.Value);
        }

        query = query.OrderByDescending(r => r.ReviewPeriodEnd);
        return await PaginatedList<PerformanceReview>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<PerformanceReview?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.PerformanceReviews
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Reviewer)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<string?> CreateAsync(PerformanceReview review, CancellationToken ct = default)
    {
        // Check for overlapping review periods
        var hasOverlap = await db.PerformanceReviews.AnyAsync(r =>
            r.EmployeeId == review.EmployeeId &&
            r.ReviewPeriodStart <= review.ReviewPeriodEnd &&
            r.ReviewPeriodEnd >= review.ReviewPeriodStart, ct);

        if (hasOverlap)
        {
            return "This employee already has a review for an overlapping period.";
        }

        review.Status = ReviewStatus.Draft;
        db.PerformanceReviews.Add(review);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Performance review created for employee {EmployeeId}", review.EmployeeId);
        return null;
    }

    public async Task<string?> SubmitSelfAssessmentAsync(int reviewId, string selfAssessment, CancellationToken ct = default)
    {
        var review = await db.PerformanceReviews.FindAsync([reviewId], ct);
        if (review is null) { return "Review not found."; }
        if (review.Status != ReviewStatus.SelfAssessmentPending)
        {
            return "This review is not awaiting self-assessment.";
        }

        review.SelfAssessment = selfAssessment;
        review.Status = ReviewStatus.ManagerReviewPending;
        await db.SaveChangesAsync(ct);
        return null;
    }

    public async Task<string?> CompleteManagerReviewAsync(int reviewId, string managerAssessment, OverallRating rating,
        string? strengths, string? areasForImprovement, string? goals, CancellationToken ct = default)
    {
        var review = await db.PerformanceReviews.FindAsync([reviewId], ct);
        if (review is null) { return "Review not found."; }
        if (review.Status != ReviewStatus.ManagerReviewPending)
        {
            return "This review is not awaiting manager review.";
        }

        review.ManagerAssessment = managerAssessment;
        review.OverallRating = rating;
        review.StrengthsNoted = strengths;
        review.AreasForImprovement = areasForImprovement;
        review.Goals = goals;
        review.CompletedDate = DateOnly.FromDateTime(DateTime.Today);
        review.Status = ReviewStatus.Completed;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Performance review {ReviewId} completed with rating {Rating}", reviewId, rating);
        return null;
    }

    public async Task<int> GetUpcomingReviewCountAsync(CancellationToken ct = default)
    {
        return await db.PerformanceReviews.CountAsync(r =>
            r.Status != ReviewStatus.Completed, ct);
    }

    public async Task<string?> StartSelfAssessmentAsync(int reviewId, CancellationToken ct = default)
    {
        var review = await db.PerformanceReviews.FindAsync([reviewId], ct);
        if (review is null) { return "Review not found."; }
        if (review.Status != ReviewStatus.Draft) { return "Only draft reviews can be started."; }

        review.Status = ReviewStatus.SelfAssessmentPending;
        await db.SaveChangesAsync(ct);
        return null;
    }
}
