using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMemberService
{
    Task<PaginatedResponse<MemberResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct);
    Task<MemberDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct);
    Task<MemberResponse?> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct);
    Task<bool> DeactivateAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<BookingResponse>> GetBookingsAsync(int memberId, string? status, DateOnly? from, DateOnly? to, int page, int pageSize, CancellationToken ct);
    Task<List<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct);
    Task<List<MembershipResponse>> GetMembershipsAsync(int memberId, CancellationToken ct);
}
