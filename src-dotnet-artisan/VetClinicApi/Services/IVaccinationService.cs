using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVaccinationService
{
    Task<VaccinationResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<(VaccinationResponse? Result, string? Error)> CreateAsync(CreateVaccinationRequest request, CancellationToken cancellationToken);
}
