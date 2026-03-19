using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PagedResult<OwnerDto>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<OwnerDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<OwnerDto> CreateAsync(CreateOwnerDto dto, CancellationToken ct = default);
    Task<OwnerDto?> UpdateAsync(int id, UpdateOwnerDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<PetSummaryDto>> GetPetsAsync(int ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<AppointmentDto>> GetAppointmentsAsync(int ownerId, CancellationToken ct = default);
    Task<bool> HasActivePetsAsync(int id, CancellationToken ct = default);
}
