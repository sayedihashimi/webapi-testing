using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PagedResult<OwnerDto>> GetAllAsync(string? search, PaginationParams pagination);
    Task<OwnerDetailDto?> GetByIdAsync(int id);
    Task<OwnerDto> CreateAsync(CreateOwnerDto dto);
    Task<OwnerDto?> UpdateAsync(int id, UpdateOwnerDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IReadOnlyList<PetDto>> GetPetsAsync(int ownerId);
    Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(int ownerId, PaginationParams pagination);
}
