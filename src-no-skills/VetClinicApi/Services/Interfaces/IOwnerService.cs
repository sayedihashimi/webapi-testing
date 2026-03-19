using VetClinicApi.DTOs.Common;
using VetClinicApi.DTOs.Owner;
using VetClinicApi.DTOs.Appointment;

namespace VetClinicApi.Services.Interfaces;

public interface IOwnerService
{
    Task<PagedResult<OwnerDto>> GetAllAsync(string? search, PaginationParams pagination);
    Task<OwnerDetailDto> GetByIdAsync(int id);
    Task<OwnerDto> CreateAsync(CreateOwnerDto dto);
    Task<OwnerDto> UpdateAsync(int id, UpdateOwnerDto dto);
    Task DeleteAsync(int id);
    Task<List<DTOs.Pet.PetDto>> GetPetsAsync(int ownerId);
    Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(int ownerId, PaginationParams pagination);
}
