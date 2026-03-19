using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVaccinationService
{
    Task<VaccinationDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<VaccinationDto> CreateAsync(CreateVaccinationDto dto, CancellationToken ct = default);
}
