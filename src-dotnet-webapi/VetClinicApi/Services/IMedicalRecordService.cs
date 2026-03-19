using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IMedicalRecordService
{
    Task<MedicalRecordDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<MedicalRecordResponse> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct);
    Task<MedicalRecordResponse?> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken ct);
}
