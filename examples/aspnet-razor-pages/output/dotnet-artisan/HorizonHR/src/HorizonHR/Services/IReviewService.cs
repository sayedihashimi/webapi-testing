using HorizonHR.Models;

namespace HorizonHR.Services;

public interface IReviewService
{
    Task<PaginatedList<PerformanceReview>> GetAllAsync(int pageNumber, int pageSize,
        ReviewStatus? status = null, OverallRating? rating = null, int? departmentId = null,
        CancellationToken ct = default);
    Task<PerformanceReview?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<string?> CreateAsync(PerformanceReview review, CancellationToken ct = default);
    Task<string?> SubmitSelfAssessmentAsync(int reviewId, string selfAssessment, CancellationToken ct = default);
    Task<string?> CompleteManagerReviewAsync(int reviewId, string managerAssessment, OverallRating rating,
        string? strengths, string? areasForImprovement, string? goals, CancellationToken ct = default);
    Task<int> GetUpcomingReviewCountAsync(CancellationToken ct = default);
    Task<string?> StartSelfAssessmentAsync(int reviewId, CancellationToken ct = default);
}
