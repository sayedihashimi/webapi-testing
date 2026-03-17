using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IMedicalRecordService
{
    Task<MedicalRecordDetailDto?> GetByIdAsync(int id);
    Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto);
    Task<MedicalRecordDto?> UpdateAsync(int id, UpdateMedicalRecordDto dto);
}
