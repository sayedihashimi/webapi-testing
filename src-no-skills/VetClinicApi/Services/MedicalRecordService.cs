using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IMedicalRecordService
{
    Task<MedicalRecordDto> GetByIdAsync(int id);
    Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto);
    Task<MedicalRecordDto> UpdateAsync(int id, UpdateMedicalRecordDto dto);
}

public class MedicalRecordService : IMedicalRecordService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<MedicalRecordService> _logger;

    public MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<MedicalRecordDto> GetByIdAsync(int id)
    {
        var record = await _db.MedicalRecords
            .Include(r => r.Prescriptions)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException("MedicalRecord", id);

        return MapToDto(record);
    }

    public async Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto)
    {
        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == dto.AppointmentId)
            ?? throw new NotFoundException("Appointment", dto.AppointmentId);

        if (appointment.Status != AppointmentStatus.Completed && appointment.Status != AppointmentStatus.InProgress)
            throw new BusinessRuleException("Medical records can only be created for appointments with status 'Completed' or 'InProgress'.");

        if (await _db.MedicalRecords.AnyAsync(r => r.AppointmentId == dto.AppointmentId))
            throw new BusinessRuleException("A medical record already exists for this appointment.", StatusCodes.Status409Conflict);

        var record = new MedicalRecord
        {
            AppointmentId = dto.AppointmentId,
            PetId = appointment.PetId,
            VeterinarianId = appointment.VeterinarianId,
            Diagnosis = dto.Diagnosis,
            Treatment = dto.Treatment,
            Notes = dto.Notes,
            FollowUpDate = dto.FollowUpDate
        };

        _db.MedicalRecords.Add(record);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created medical record {RecordId} for appointment {AppointmentId}", record.Id, record.AppointmentId);
        return MapToDto(record);
    }

    public async Task<MedicalRecordDto> UpdateAsync(int id, UpdateMedicalRecordDto dto)
    {
        var record = await _db.MedicalRecords
            .Include(r => r.Prescriptions)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException("MedicalRecord", id);

        record.Diagnosis = dto.Diagnosis;
        record.Treatment = dto.Treatment;
        record.Notes = dto.Notes;
        record.FollowUpDate = dto.FollowUpDate;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated medical record {RecordId}", record.Id);
        return MapToDto(record);
    }

    private static MedicalRecordDto MapToDto(MedicalRecord r) => new()
    {
        Id = r.Id, AppointmentId = r.AppointmentId, PetId = r.PetId,
        VeterinarianId = r.VeterinarianId, Diagnosis = r.Diagnosis,
        Treatment = r.Treatment, Notes = r.Notes, FollowUpDate = r.FollowUpDate,
        CreatedAt = r.CreatedAt,
        Prescriptions = r.Prescriptions.Select(p => new PrescriptionDto
        {
            Id = p.Id, MedicalRecordId = p.MedicalRecordId,
            MedicationName = p.MedicationName, Dosage = p.Dosage,
            DurationDays = p.DurationDays, StartDate = p.StartDate,
            EndDate = p.EndDate, Instructions = p.Instructions,
            IsActive = p.IsActive, CreatedAt = p.CreatedAt
        }).ToList()
    };
}
