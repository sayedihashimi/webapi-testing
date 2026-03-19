using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMemberService
{
    Task<PaginatedResponse<MemberResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct);
    Task<MemberResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct);
    Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<BookingResponse>> GetBookingsAsync(int memberId, CancellationToken ct);
    Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct);
    Task<IReadOnlyList<MembershipResponse>> GetMembershipsAsync(int memberId, CancellationToken ct);
}
