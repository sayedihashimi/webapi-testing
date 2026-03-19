using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPrescriptionService
{
    Task<PrescriptionResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<PrescriptionResponse> CreateAsync(CreatePrescriptionRequest request, CancellationToken ct);
}
