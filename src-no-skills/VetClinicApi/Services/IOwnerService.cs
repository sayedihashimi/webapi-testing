using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PagedResponse<OwnerResponseDto>> GetAllAsync(string? search, PaginationParams pagination);
    Task<OwnerResponseDto?> GetByIdAsync(int id);
    Task<OwnerResponseDto> CreateAsync(CreateOwnerDto dto);
    Task<OwnerResponseDto> UpdateAsync(int id, UpdateOwnerDto dto);
    Task DeleteAsync(int id);
    Task<List<PetResponseDto>> GetPetsAsync(int ownerId);
    Task<PagedResponse<AppointmentResponseDto>> GetAppointmentsAsync(int ownerId, PaginationParams pagination);
}
