using HorizonHR.Models;
using HorizonHR.Models.Enums;

namespace HorizonHR.Services.Interfaces;

public interface IReviewService
{
    Task<(List<PerformanceReview> Items, int TotalCount)> GetReviewsAsync(ReviewStatus? status, OverallRating? rating, int? departmentId, int page, int pageSize);
    Task<PerformanceReview?> GetReviewByIdAsync(int id);
    Task<PerformanceReview> CreateReviewAsync(PerformanceReview review);
    Task SubmitSelfAssessmentAsync(int reviewId, string selfAssessment);
    Task CompleteManagerReviewAsync(int reviewId, string managerAssessment, OverallRating rating, string? strengths, string? areasForImprovement, string? goals);
    Task StartReviewAsync(int reviewId);
}
