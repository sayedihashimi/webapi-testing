using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PagedResult<OwnerResponseDto>> GetAllAsync(string? search, PaginationParams pagination);
    Task<OwnerDetailDto?> GetByIdAsync(int id);
    Task<OwnerResponseDto> CreateAsync(OwnerCreateDto dto);
    Task<OwnerResponseDto?> UpdateAsync(int id, OwnerUpdateDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
    Task<IEnumerable<PetResponseDto>> GetPetsAsync(int ownerId);
    Task<PagedResult<AppointmentResponseDto>> GetAppointmentsAsync(int ownerId, PaginationParams pagination);
}

public interface IPetService
{
    Task<PagedResult<PetResponseDto>> GetAllAsync(string? search, string? species, bool includeInactive, PaginationParams pagination);
    Task<PetDetailDto?> GetByIdAsync(int id);
    Task<PetResponseDto> CreateAsync(PetCreateDto dto);
    Task<PetResponseDto?> UpdateAsync(int id, PetUpdateDto dto);
    Task<bool> SoftDeleteAsync(int id);
    Task<IEnumerable<MedicalRecordResponseDto>> GetMedicalRecordsAsync(int petId);
    Task<IEnumerable<VaccinationResponseDto>> GetVaccinationsAsync(int petId);
    Task<IEnumerable<VaccinationResponseDto>> GetUpcomingVaccinationsAsync(int petId);
    Task<IEnumerable<PrescriptionResponseDto>> GetActivePrescriptionsAsync(int petId);
}

public interface IVeterinarianService
{
    Task<PagedResult<VetResponseDto>> GetAllAsync(string? specialization, bool? isAvailable, PaginationParams pagination);
    Task<VetResponseDto?> GetByIdAsync(int id);
    Task<VetResponseDto> CreateAsync(VetCreateDto dto);
    Task<VetResponseDto?> UpdateAsync(int id, VetUpdateDto dto);
    Task<IEnumerable<AppointmentResponseDto>> GetScheduleAsync(int vetId, DateOnly date);
    Task<PagedResult<AppointmentResponseDto>> GetAppointmentsAsync(int vetId, string? status, PaginationParams pagination);
}

public interface IAppointmentService
{
    Task<PagedResult<AppointmentResponseDto>> GetAllAsync(DateTime? from, DateTime? to, string? status, int? vetId, int? petId, PaginationParams pagination);
    Task<AppointmentDetailDto?> GetByIdAsync(int id);
    Task<(AppointmentResponseDto? Result, string? Error)> CreateAsync(AppointmentCreateDto dto);
    Task<(AppointmentResponseDto? Result, string? Error)> UpdateAsync(int id, AppointmentUpdateDto dto);
    Task<(AppointmentResponseDto? Result, string? Error)> UpdateStatusAsync(int id, AppointmentStatusUpdateDto dto);
    Task<IEnumerable<AppointmentResponseDto>> GetTodayAsync();
}

public interface IMedicalRecordService
{
    Task<MedicalRecordResponseDto?> GetByIdAsync(int id);
    Task<(MedicalRecordResponseDto? Result, string? Error)> CreateAsync(MedicalRecordCreateDto dto);
    Task<MedicalRecordResponseDto?> UpdateAsync(int id, MedicalRecordUpdateDto dto);
}

public interface IPrescriptionService
{
    Task<PrescriptionResponseDto?> GetByIdAsync(int id);
    Task<(PrescriptionResponseDto? Result, string? Error)> CreateAsync(PrescriptionCreateDto dto);
}

public interface IVaccinationService
{
    Task<VaccinationResponseDto?> GetByIdAsync(int id);
    Task<(VaccinationResponseDto? Result, string? Error)> CreateAsync(VaccinationCreateDto dto);
}
