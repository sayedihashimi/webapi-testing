using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IMedicalRecordService
{
    Task<MedicalRecordDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<(MedicalRecordResponse? Result, string? Error)> CreateAsync(CreateMedicalRecordRequest request, CancellationToken cancellationToken);
    Task<(MedicalRecordResponse? Result, string? Error, bool NotFound)> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken cancellationToken);
}
