using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IMedicalRecordService
{
    Task<MedicalRecordResponseDto?> GetByIdAsync(int id);
    Task<MedicalRecordResponseDto> CreateAsync(CreateMedicalRecordDto dto);
    Task<MedicalRecordResponseDto> UpdateAsync(int id, UpdateMedicalRecordDto dto);
}
