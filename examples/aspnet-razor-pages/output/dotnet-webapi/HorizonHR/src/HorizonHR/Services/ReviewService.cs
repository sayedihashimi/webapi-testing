using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public sealed class ReviewService(HorizonDbContext db, ILogger<ReviewService> logger) : IReviewService
{
    public async Task<PaginatedList<PerformanceReview>> GetAllAsync(ReviewStatus? status, OverallRating? rating, int? departmentId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.PerformanceReviews
            .AsNoTracking()
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Reviewer)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);
        if (rating.HasValue)
            query = query.Where(r => r.OverallRating == rating.Value);
        if (departmentId.HasValue)
            query = query.Where(r => r.Employee.DepartmentId == departmentId.Value);

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

    public async Task<PerformanceReview> CreateAsync(int employeeId, int reviewerId, DateOnly periodStart, DateOnly periodEnd, CancellationToken ct = default)
    {
        if (periodEnd <= periodStart)
            throw new InvalidOperationException("Review period end must be after start.");

        // Check for overlapping review periods
        var hasOverlap = await db.PerformanceReviews.AnyAsync(r =>
            r.EmployeeId == employeeId &&
            r.ReviewPeriodStart <= periodEnd && r.ReviewPeriodEnd >= periodStart, ct);

        if (hasOverlap)
            throw new InvalidOperationException("This employee already has a review with an overlapping period.");

        var review = new PerformanceReview
        {
            EmployeeId = employeeId,
            ReviewerId = reviewerId,
            ReviewPeriodStart = periodStart,
            ReviewPeriodEnd = periodEnd,
            Status = ReviewStatus.Draft
        };

        db.PerformanceReviews.Add(review);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Performance review created for employee {EmployeeId}", employeeId);
        return review;
    }

    public async Task<PerformanceReview> SubmitSelfAssessmentAsync(int id, string selfAssessment, CancellationToken ct = default)
    {
        var review = await db.PerformanceReviews.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Review not found.");

        if (review.Status != ReviewStatus.SelfAssessmentPending)
            throw new InvalidOperationException("Review is not in SelfAssessmentPending status.");

        review.SelfAssessment = selfAssessment;
        review.Status = ReviewStatus.ManagerReviewPending;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Self-assessment submitted for review {ReviewId}", id);
        return review;
    }

    public async Task<PerformanceReview> CompleteManagerReviewAsync(int id, string managerAssessment, OverallRating rating, string? strengths, string? areasForImprovement, string? goals, CancellationToken ct = default)
    {
        var review = await db.PerformanceReviews.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Review not found.");

        if (review.Status != ReviewStatus.ManagerReviewPending)
            throw new InvalidOperationException("Review is not in ManagerReviewPending status.");

        review.ManagerAssessment = managerAssessment;
        review.OverallRating = rating;
        review.StrengthsNoted = strengths;
        review.AreasForImprovement = areasForImprovement;
        review.Goals = goals;
        review.CompletedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        review.Status = ReviewStatus.Completed;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Review completed: {ReviewId} with rating {Rating}", id, rating);
        return review;
    }
}
