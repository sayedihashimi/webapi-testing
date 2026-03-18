using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMemberService
{
    Task<PagedResult<MemberDto>> GetAllAsync(string? search, bool? isActive, int page, int pageSize);
    Task<MemberDetailDto> GetByIdAsync(int id);
    Task<MemberDto> CreateAsync(CreateMemberDto dto);
    Task<MemberDto> UpdateAsync(int id, UpdateMemberDto dto);
    Task DeleteAsync(int id);
    Task<PagedResult<BookingDto>> GetBookingsAsync(int memberId, string? status, DateTime? from, DateTime? to, int page, int pageSize);
    Task<List<BookingDto>> GetUpcomingBookingsAsync(int memberId);
    Task<List<MembershipDto>> GetMembershipsAsync(int memberId);
}
