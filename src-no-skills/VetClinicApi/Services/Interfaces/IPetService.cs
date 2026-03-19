using VetClinicApi.DTOs.Common;
using VetClinicApi.DTOs.Pet;
using VetClinicApi.DTOs.MedicalRecord;
using VetClinicApi.DTOs.Vaccination;
using VetClinicApi.DTOs.Prescription;

namespace VetClinicApi.Services.Interfaces;

public interface IPetService
{
    Task<PagedResult<PetDto>> GetAllAsync(string? search, bool includeInactive, PaginationParams pagination);
    Task<PetDetailDto> GetByIdAsync(int id);
    Task<PetDto> CreateAsync(CreatePetDto dto);
    Task<PetDto> UpdateAsync(int id, UpdatePetDto dto);
    Task DeleteAsync(int id);
    Task<List<MedicalRecordDto>> GetMedicalRecordsAsync(int petId);
    Task<List<VaccinationDto>> GetVaccinationsAsync(int petId);
    Task<List<VaccinationDto>> GetUpcomingVaccinationsAsync(int petId);
    Task<List<PrescriptionDto>> GetActivePrescriptionsAsync(int petId);
}
