using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVaccinationService
{
    Task<VaccinationResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<VaccinationResponse> CreateAsync(CreateVaccinationRequest request, CancellationToken ct);
}
