using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMembershipPlanService
{
    Task<List<MembershipPlanResponse>> GetAllActivePlansAsync(CancellationToken ct = default);
    Task<MembershipPlanResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MembershipPlanResponse> CreateAsync(CreateMembershipPlanRequest request, CancellationToken ct = default);
    Task<MembershipPlanResponse?> UpdateAsync(int id, UpdateMembershipPlanRequest request, CancellationToken ct = default);
    Task<bool> DeactivateAsync(int id, CancellationToken ct = default);
}
