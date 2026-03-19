using VetClinicApi.DTOs.Vaccination;

namespace VetClinicApi.Services.Interfaces;

public interface IVaccinationService
{
    Task<VaccinationDto> GetByIdAsync(int id);
    Task<VaccinationDto> CreateAsync(CreateVaccinationDto dto);
}
