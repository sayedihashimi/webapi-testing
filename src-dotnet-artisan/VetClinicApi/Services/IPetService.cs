using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPetService
{
    Task<PagedResult<PetDto>> GetAllAsync(string? search, int page, int pageSize, bool includeInactive, CancellationToken ct = default);
    Task<PetDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PetDto> CreateAsync(CreatePetDto dto, CancellationToken ct = default);
    Task<PetDto?> UpdateAsync(int id, UpdatePetDto dto, CancellationToken ct = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<MedicalRecordDto>> GetMedicalRecordsAsync(int petId, CancellationToken ct = default);
    Task<IReadOnlyList<VaccinationDto>> GetVaccinationsAsync(int petId, CancellationToken ct = default);
    Task<IReadOnlyList<VaccinationDto>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct = default);
    Task<IReadOnlyList<PrescriptionDto>> GetActivePrescriptionsAsync(int petId, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
