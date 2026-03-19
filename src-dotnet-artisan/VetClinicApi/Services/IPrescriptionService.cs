using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPrescriptionService
{
    Task<PrescriptionResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<(PrescriptionResponse? Result, string? Error)> CreateAsync(CreatePrescriptionRequest request, CancellationToken cancellationToken);
}
