using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMembershipService
{
    Task<MembershipResponse> CreateAsync(CreateMembershipRequest request, CancellationToken ct = default);
    Task<MembershipResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MembershipResponse> CancelAsync(int id, CancellationToken ct = default);
    Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct = default);
    Task<MembershipResponse> UnfreezeAsync(int id, CancellationToken ct = default);
    Task<MembershipResponse> RenewAsync(int id, CancellationToken ct = default);
}
