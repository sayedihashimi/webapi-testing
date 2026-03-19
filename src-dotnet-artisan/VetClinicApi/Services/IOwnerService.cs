using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PagedResult<OwnerResponse>> GetAllAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);
    Task<OwnerDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<OwnerResponse> CreateAsync(CreateOwnerRequest request, CancellationToken cancellationToken);
    Task<OwnerResponse?> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken cancellationToken);
    Task<(bool Found, bool HasActivePets)> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<PetResponse>> GetPetsAsync(int ownerId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AppointmentResponse>> GetAppointmentsAsync(int ownerId, CancellationToken cancellationToken);
}
