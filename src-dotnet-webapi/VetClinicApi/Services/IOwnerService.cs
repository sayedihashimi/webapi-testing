using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PaginatedResponse<OwnerResponse>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct);
    Task<OwnerDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<OwnerResponse> CreateAsync(CreateOwnerRequest request, CancellationToken ct);
    Task<OwnerResponse?> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<PetSummaryResponse>> GetPetsAsync(int ownerId, CancellationToken ct);
    Task<PaginatedResponse<AppointmentResponse>> GetAppointmentsAsync(int ownerId, int page, int pageSize, CancellationToken ct);
}
