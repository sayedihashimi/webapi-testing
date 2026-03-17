using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPrescriptionService
{
    Task<PrescriptionDto?> GetByIdAsync(int id);
    Task<PrescriptionDto> CreateAsync(CreatePrescriptionDto dto);
}
