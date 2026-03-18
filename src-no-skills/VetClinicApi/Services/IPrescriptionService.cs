using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPrescriptionService
{
    Task<PrescriptionResponseDto?> GetByIdAsync(int id);
    Task<PrescriptionResponseDto> CreateAsync(CreatePrescriptionDto dto);
}
