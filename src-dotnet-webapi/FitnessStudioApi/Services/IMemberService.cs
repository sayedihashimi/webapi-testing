using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMemberService
{
    Task<PagedResponse<MemberResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct);
    Task<MemberDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct);
    Task<MemberResponse?> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct);
    Task<(bool Success, string? Error)> DeactivateAsync(int id, CancellationToken ct);
    Task<PagedResponse<BookingResponse>> GetMemberBookingsAsync(int memberId, string? status, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize, CancellationToken ct);
    Task<List<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct);
    Task<List<MembershipResponse>> GetMemberMembershipsAsync(int memberId, CancellationToken ct);
}
