using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPetService
{
    Task<PagedResponse<PetResponse>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct);
    Task<PetResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct);
    Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    Task<PagedResponse<MedicalRecordResponse>> GetMedicalRecordsAsync(int petId, int page, int pageSize, CancellationToken ct);
    Task<List<VaccinationResponse>> GetVaccinationsAsync(int petId, CancellationToken ct);
    Task<List<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct);
    Task<List<PrescriptionResponse>> GetActivePrescriptionsAsync(int petId, CancellationToken ct);
}
