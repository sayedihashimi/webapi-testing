using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IMedicalRecordService
{
    Task<MedicalRecordDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto, CancellationToken ct = default);
    Task<MedicalRecordDto?> UpdateAsync(int id, UpdateMedicalRecordDto dto, CancellationToken ct = default);
    Task<string?> ValidateAppointmentForRecordAsync(int appointmentId, CancellationToken ct = default);
}
