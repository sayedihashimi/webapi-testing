using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IMemberService
{
    Task<PagedResult<MemberResponseDto>> GetAllAsync(string? search, bool? isActive, PaginationParams pagination);
    Task<MemberResponseDto?> GetByIdAsync(int id);
    Task<MemberResponseDto> CreateAsync(MemberCreateDto dto);
    Task<MemberResponseDto?> UpdateAsync(int id, MemberUpdateDto dto);
    Task<bool> DeactivateAsync(int id);
    Task<PagedResult<BookingResponseDto>> GetBookingsAsync(int memberId, string? status, DateTime? fromDate, DateTime? toDate, PaginationParams pagination);
    Task<List<BookingResponseDto>> GetUpcomingBookingsAsync(int memberId);
    Task<List<MembershipResponseDto>> GetMembershipsAsync(int memberId);
}
