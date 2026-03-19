using VetClinicApi.DTOs.Prescription;

namespace VetClinicApi.Services.Interfaces;

public interface IPrescriptionService
{
    Task<PrescriptionDto> GetByIdAsync(int id);
    Task<PrescriptionDto> CreateAsync(CreatePrescriptionDto dto);
}
