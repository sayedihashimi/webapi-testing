using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMemberService
{
    Task<PagedResult<MemberResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct = default);
    Task<MemberDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct = default);
    Task<MemberResponse?> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct = default);
    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken ct = default);
    Task<PagedResult<BookingResponse>> GetMemberBookingsAsync(int memberId, string? status, int page, int pageSize, CancellationToken ct = default);
    Task<List<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct = default);
    Task<List<MembershipResponse>> GetMemberMembershipsAsync(int memberId, CancellationToken ct = default);
}
