using VetClinicApi.DTOs.MedicalRecord;

namespace VetClinicApi.Services.Interfaces;

public interface IMedicalRecordService
{
    Task<MedicalRecordDto> GetByIdAsync(int id);
    Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto);
    Task<MedicalRecordDto> UpdateAsync(int id, UpdateMedicalRecordDto dto);
}
