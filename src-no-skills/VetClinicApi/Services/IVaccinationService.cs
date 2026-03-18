using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVaccinationService
{
    Task<VaccinationResponseDto?> GetByIdAsync(int id);
    Task<VaccinationResponseDto> CreateAsync(CreateVaccinationDto dto);
}
