using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PagedResponse<OwnerSummaryResponse>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct);
    Task<OwnerResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<OwnerResponse> CreateAsync(CreateOwnerRequest request, CancellationToken ct);
    Task<OwnerResponse?> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<PagedResponse<PetSummaryResponse>> GetOwnerPetsAsync(int ownerId, CancellationToken ct);
    Task<PagedResponse<AppointmentResponse>> GetOwnerAppointmentsAsync(int ownerId, int page, int pageSize, CancellationToken ct);
}
