using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPetService
{
    Task<PagedResult<PetDto>> GetAllAsync(string? search, string? species, bool includeInactive, PaginationParams pagination);
    Task<PetDetailDto?> GetByIdAsync(int id);
    Task<PetDto> CreateAsync(CreatePetDto dto);
    Task<PetDto?> UpdateAsync(int id, UpdatePetDto dto);
    Task<bool> SoftDeleteAsync(int id);
    Task<IReadOnlyList<MedicalRecordDto>> GetMedicalRecordsAsync(int petId);
    Task<IReadOnlyList<VaccinationDto>> GetVaccinationsAsync(int petId);
    Task<IReadOnlyList<VaccinationDto>> GetUpcomingVaccinationsAsync(int petId);
    Task<IReadOnlyList<PrescriptionDto>> GetActivePrescriptionsAsync(int petId);
}
