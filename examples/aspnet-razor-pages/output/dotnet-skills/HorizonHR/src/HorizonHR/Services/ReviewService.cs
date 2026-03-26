using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HorizonHR.Services;

public sealed class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(ApplicationDbContext context, ILogger<ReviewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<PerformanceReview> Items, int TotalCount)> GetReviewsAsync(
        ReviewStatus? status, OverallRating? rating, int? departmentId, int page, int pageSize)
    {
        var query = _context.PerformanceReviews
            .AsNoTracking()
            .Include(r => r.Employee)
                .ThenInclude(e => e.Department)
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

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.ReviewPeriodEnd)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<PerformanceReview?> GetReviewByIdAsync(int id)
    {
        return await _context.PerformanceReviews
            .AsNoTracking()
            .Include(r => r.Employee)
            .Include(r => r.Reviewer)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<PerformanceReview> CreateReviewAsync(PerformanceReview review)
    {
        // Validate no overlapping review periods for the same employee
        var hasOverlap = await _context.PerformanceReviews
            .AsNoTracking()
            .AnyAsync(r =>
                r.EmployeeId == review.EmployeeId &&
                r.ReviewPeriodStart <= review.ReviewPeriodEnd &&
                r.ReviewPeriodEnd >= review.ReviewPeriodStart);

        if (hasOverlap)
        {
            throw new InvalidOperationException(
                "An overlapping review period already exists for this employee.");
        }

        review.Status = ReviewStatus.Draft;
        review.CreatedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created performance review {ReviewId} for employee {EmployeeId}",
            review.Id, review.EmployeeId);

        return review;
    }

    public async Task SubmitSelfAssessmentAsync(int reviewId, string selfAssessment)
    {
        var review = await _context.PerformanceReviews
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
        {
            throw new InvalidOperationException($"Performance review with ID {reviewId} not found.");
        }

        if (review.Status != ReviewStatus.Draft && review.Status != ReviewStatus.SelfAssessmentPending)
        {
            throw new InvalidOperationException(
                "Self-assessment can only be submitted for reviews in Draft or SelfAssessmentPending status.");
        }

        if (string.IsNullOrWhiteSpace(selfAssessment))
        {
            throw new InvalidOperationException("Self-assessment text is required.");
        }

        review.SelfAssessment = selfAssessment;
        review.Status = ReviewStatus.ManagerReviewPending;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Self-assessment submitted for review {ReviewId}. Status transitioned to ManagerReviewPending",
            reviewId);
    }

    public async Task CompleteManagerReviewAsync(int reviewId, string managerAssessment,
        OverallRating rating, string? strengths, string? areasForImprovement, string? goals)
    {
        var review = await _context.PerformanceReviews
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
        {
            throw new InvalidOperationException($"Performance review with ID {reviewId} not found.");
        }

        if (review.Status != ReviewStatus.ManagerReviewPending)
        {
            throw new InvalidOperationException(
                "Manager review can only be completed for reviews in ManagerReviewPending status.");
        }

        review.ManagerAssessment = managerAssessment;
        review.OverallRating = rating;
        review.StrengthsNoted = strengths;
        review.AreasForImprovement = areasForImprovement;
        review.Goals = goals;
        review.CompletedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        review.Status = ReviewStatus.Completed;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Manager review completed for review {ReviewId}. Rating: {Rating}",
            reviewId, rating);
    }

    public async Task StartReviewAsync(int reviewId)
    {
        var review = await _context.PerformanceReviews
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
        {
            throw new InvalidOperationException($"Performance review with ID {reviewId} not found.");
        }

        if (review.Status != ReviewStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only reviews in Draft status can be started.");
        }

        review.Status = ReviewStatus.SelfAssessmentPending;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Review {ReviewId} started. Status transitioned to SelfAssessmentPending",
            reviewId);
    }
}
