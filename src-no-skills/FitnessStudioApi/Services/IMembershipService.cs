using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMembershipService
{
    Task<MembershipDto> GetByIdAsync(int id);
    Task<MembershipDto> CreateAsync(CreateMembershipDto dto);
    Task<MembershipDto> CancelAsync(int id);
    Task<MembershipDto> FreezeAsync(int id, FreezeMembershipDto dto);
    Task<MembershipDto> UnfreezeAsync(int id);
    Task<MembershipDto> RenewAsync(int id);
}
