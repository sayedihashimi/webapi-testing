using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPetService
{
    Task<PagedResult<PetResponse>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken cancellationToken);
    Task<PetDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<PetResponse?> CreateAsync(CreatePetRequest request, CancellationToken cancellationToken);
    Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken cancellationToken);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<MedicalRecordResponse>> GetMedicalRecordsAsync(int petId, CancellationToken cancellationToken);
    Task<IReadOnlyList<VaccinationResponse>> GetVaccinationsAsync(int petId, CancellationToken cancellationToken);
    Task<IReadOnlyList<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PrescriptionResponse>> GetActivePrescriptionsAsync(int petId, CancellationToken cancellationToken);
}
