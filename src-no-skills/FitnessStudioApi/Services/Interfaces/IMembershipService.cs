using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IMembershipService
{
    Task<MembershipResponseDto> CreateAsync(MembershipCreateDto dto);
    Task<MembershipResponseDto?> GetByIdAsync(int id);
    Task<MembershipResponseDto> CancelAsync(int id);
    Task<MembershipResponseDto> FreezeAsync(int id, FreezeMembershipDto dto);
    Task<MembershipResponseDto> UnfreezeAsync(int id);
    Task<MembershipResponseDto> RenewAsync(int id);
}
