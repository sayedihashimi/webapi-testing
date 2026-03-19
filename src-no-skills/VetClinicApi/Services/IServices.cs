using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PaginatedResponse<OwnerSummaryDto>> GetAllAsync(string? search, int page, int pageSize);
    Task<OwnerResponseDto> GetByIdAsync(int id);
    Task<OwnerResponseDto> CreateAsync(CreateOwnerDto dto);
    Task<OwnerResponseDto> UpdateAsync(int id, UpdateOwnerDto dto);
    Task DeleteAsync(int id);
    Task<List<PetResponseDto>> GetPetsAsync(int ownerId);
    Task<PaginatedResponse<AppointmentResponseDto>> GetAppointmentsAsync(int ownerId, int page, int pageSize);
}

public interface IPetService
{
    Task<PaginatedResponse<PetResponseDto>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize);
    Task<PetResponseDto> GetByIdAsync(int id);
    Task<PetResponseDto> CreateAsync(CreatePetDto dto);
    Task<PetResponseDto> UpdateAsync(int id, UpdatePetDto dto);
    Task DeleteAsync(int id);
    Task<List<MedicalRecordResponseDto>> GetMedicalRecordsAsync(int petId);
    Task<List<VaccinationResponseDto>> GetVaccinationsAsync(int petId);
    Task<List<VaccinationResponseDto>> GetUpcomingVaccinationsAsync(int petId);
    Task<List<PrescriptionResponseDto>> GetActivePrescriptionsAsync(int petId);
}

public interface IVeterinarianService
{
    Task<PaginatedResponse<VeterinarianResponseDto>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize);
    Task<VeterinarianResponseDto> GetByIdAsync(int id);
    Task<VeterinarianResponseDto> CreateAsync(CreateVeterinarianDto dto);
    Task<VeterinarianResponseDto> UpdateAsync(int id, UpdateVeterinarianDto dto);
    Task<List<AppointmentResponseDto>> GetScheduleAsync(int vetId, DateOnly date);
    Task<PaginatedResponse<AppointmentResponseDto>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize);
}

public interface IAppointmentService
{
    Task<PaginatedResponse<AppointmentResponseDto>> GetAllAsync(DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId, int page, int pageSize);
    Task<AppointmentResponseDto> GetByIdAsync(int id);
    Task<AppointmentResponseDto> CreateAsync(CreateAppointmentDto dto);
    Task<AppointmentResponseDto> UpdateAsync(int id, UpdateAppointmentDto dto);
    Task<AppointmentResponseDto> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto);
    Task<List<AppointmentResponseDto>> GetTodayAsync();
}

public interface IMedicalRecordService
{
    Task<MedicalRecordResponseDto> GetByIdAsync(int id);
    Task<MedicalRecordResponseDto> CreateAsync(CreateMedicalRecordDto dto);
    Task<MedicalRecordResponseDto> UpdateAsync(int id, UpdateMedicalRecordDto dto);
}

public interface IPrescriptionService
{
    Task<PrescriptionResponseDto> GetByIdAsync(int id);
    Task<PrescriptionResponseDto> CreateAsync(CreatePrescriptionDto dto);
}

public interface IVaccinationService
{
    Task<VaccinationResponseDto> GetByIdAsync(int id);
    Task<VaccinationResponseDto> CreateAsync(CreateVaccinationDto dto);
}
