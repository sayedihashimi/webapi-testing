using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IMembershipPlanService
{
    Task<List<MembershipPlanResponseDto>> GetAllActiveAsync();
    Task<MembershipPlanResponseDto?> GetByIdAsync(int id);
    Task<MembershipPlanResponseDto> CreateAsync(MembershipPlanCreateDto dto);
    Task<MembershipPlanResponseDto?> UpdateAsync(int id, MembershipPlanUpdateDto dto);
    Task<bool> DeactivateAsync(int id);
}
