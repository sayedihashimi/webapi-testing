using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVaccinationService
{
    Task<VaccinationDto?> GetByIdAsync(int id);
    Task<VaccinationDto> CreateAsync(CreateVaccinationDto dto);
}
