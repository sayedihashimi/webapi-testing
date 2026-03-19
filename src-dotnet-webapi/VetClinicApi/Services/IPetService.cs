using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPetService
{
    Task<PaginatedResponse<PetResponse>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct);
    Task<PetDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct);
    Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<MedicalRecordResponse>> GetMedicalRecordsAsync(int petId, CancellationToken ct);
    Task<IReadOnlyList<VaccinationResponse>> GetVaccinationsAsync(int petId, CancellationToken ct);
    Task<IReadOnlyList<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct);
    Task<IReadOnlyList<PrescriptionResponse>> GetActivePrescriptionsAsync(int petId, CancellationToken ct);
}
