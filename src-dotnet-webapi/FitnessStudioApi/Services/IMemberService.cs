using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMemberService
{
    Task<PaginatedResponse<MemberListResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct);
    Task<MemberResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct);
    Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<BookingResponse>> GetMemberBookingsAsync(int memberId, string? status, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct);
    Task<IReadOnlyList<MembershipResponse>> GetMembershipHistoryAsync(int memberId, CancellationToken ct);
}
