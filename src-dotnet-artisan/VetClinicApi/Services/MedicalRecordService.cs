using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger) : IMedicalRecordService
{
    public async Task<MedicalRecordDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var record = await db.MedicalRecords
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (record is null)
        {
            return null;
        }

        var prescriptionDtos = record.Prescriptions
            .Select(p => new PrescriptionDto(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.IsActive,
                p.Instructions, p.CreatedAt))
            .ToList();

        return new MedicalRecordDetailDto(
            record.Id, record.AppointmentId, record.PetId, record.VeterinarianId,
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate,
            record.CreatedAt, prescriptionDtos);
    }

    public async Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto, CancellationToken ct = default)
    {
        var appointment = await db.Appointments.FindAsync([dto.AppointmentId], ct);

        var record = new MedicalRecord
        {
            AppointmentId = dto.AppointmentId,
            PetId = appointment!.PetId,
            VeterinarianId = appointment.VeterinarianId,
            Diagnosis = dto.Diagnosis,
            Treatment = dto.Treatment,
            Notes = dto.Notes,
            FollowUpDate = dto.FollowUpDate
        };

        db.MedicalRecords.Add(record);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created medical record {RecordId} for appointment {AppointmentId}",
            record.Id, record.AppointmentId);

        return new MedicalRecordDto(
            record.Id, record.AppointmentId, record.PetId, record.VeterinarianId,
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate, record.CreatedAt);
    }

    public async Task<MedicalRecordDto?> UpdateAsync(int id, UpdateMedicalRecordDto dto, CancellationToken ct = default)
    {
        var record = await db.MedicalRecords.FindAsync([id], ct);
        if (record is null)
        {
            return null;
        }

        record.Diagnosis = dto.Diagnosis;
        record.Treatment = dto.Treatment;
        record.Notes = dto.Notes;
        record.FollowUpDate = dto.FollowUpDate;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated medical record {RecordId}", record.Id);

        return new MedicalRecordDto(
            record.Id, record.AppointmentId, record.PetId, record.VeterinarianId,
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate, record.CreatedAt);
    }

    public async Task<string?> ValidateAppointmentForRecordAsync(int appointmentId, CancellationToken ct = default)
    {
        var appointment = await db.Appointments.FindAsync([appointmentId], ct);
        if (appointment is null)
        {
            return "Appointment not found.";
        }

        if (appointment.Status is not (AppointmentStatus.Completed or AppointmentStatus.InProgress))
        {
            return $"Medical records can only be created for Completed or InProgress appointments. Current status: {appointment.Status}.";
        }

        var existingRecord = await db.MedicalRecords.AnyAsync(m => m.AppointmentId == appointmentId, ct);
        if (existingRecord)
        {
            return "A medical record already exists for this appointment.";
        }

        return null;
    }
}
