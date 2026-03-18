using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<MedicalRecordService> _logger;

    public MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<MedicalRecordResponseDto?> GetByIdAsync(int id)
    {
        var record = await _db.MedicalRecords
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id);
        return record?.ToResponseDto();
    }

    public async Task<(MedicalRecordResponseDto? Result, string? Error)> CreateAsync(MedicalRecordCreateDto dto)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

        if (appointment is null)
            return (null, "Appointment not found.");

        if (appointment.Status != AppointmentStatus.Completed && appointment.Status != AppointmentStatus.InProgress)
            return (null, $"Medical records can only be created for appointments with status 'Completed' or 'InProgress'. Current status: '{appointment.Status}'.");

        var exists = await _db.MedicalRecords.AnyAsync(m => m.AppointmentId == dto.AppointmentId);
        if (exists)
            return (null, "A medical record already exists for this appointment.");

        var record = new MedicalRecord
        {
            AppointmentId = dto.AppointmentId,
            PetId = appointment.PetId,
            VeterinarianId = appointment.VeterinarianId,
            Diagnosis = dto.Diagnosis,
            Treatment = dto.Treatment,
            Notes = dto.Notes,
            FollowUpDate = dto.FollowUpDate,
            CreatedAt = DateTime.UtcNow
        };

        _db.MedicalRecords.Add(record);
        await _db.SaveChangesAsync();

        await _db.Entry(record).Reference(m => m.Pet).LoadAsync();
        await _db.Entry(record).Reference(m => m.Veterinarian).LoadAsync();
        await _db.Entry(record).Collection(m => m.Prescriptions).LoadAsync();

        _logger.LogInformation("Medical record created: {RecordId} for appointment {ApptId}", record.Id, record.AppointmentId);
        return (record.ToResponseDto(), null);
    }

    public async Task<MedicalRecordResponseDto?> UpdateAsync(int id, MedicalRecordUpdateDto dto)
    {
        var record = await _db.MedicalRecords
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (record is null) return null;

        record.Diagnosis = dto.Diagnosis;
        record.Treatment = dto.Treatment;
        record.Notes = dto.Notes;
        record.FollowUpDate = dto.FollowUpDate;
        await _db.SaveChangesAsync();

        return record.ToResponseDto();
    }
}
