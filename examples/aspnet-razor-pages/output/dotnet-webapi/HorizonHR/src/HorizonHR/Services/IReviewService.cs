using HorizonHR.Models;
using HorizonHR.Models.Enums;

namespace HorizonHR.Services;

public interface IReviewService
{
    Task<PaginatedList<PerformanceReview>> GetAllAsync(ReviewStatus? status, OverallRating? rating, int? departmentId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<PerformanceReview?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PerformanceReview> CreateAsync(int employeeId, int reviewerId, DateOnly periodStart, DateOnly periodEnd, CancellationToken ct = default);
    Task<PerformanceReview> SubmitSelfAssessmentAsync(int id, string selfAssessment, CancellationToken ct = default);
    Task<PerformanceReview> CompleteManagerReviewAsync(int id, string managerAssessment, OverallRating rating, string? strengths, string? areasForImprovement, string? goals, CancellationToken ct = default);
}
