using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPrescriptionService
{
    Task<PrescriptionDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PrescriptionDto> CreateAsync(CreatePrescriptionDto dto, CancellationToken ct = default);
}
