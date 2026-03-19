using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger)
    : IMedicalRecordService
{
    public async Task<MedicalRecordDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var record = await db.MedicalRecords.AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (record is null) return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var prescriptions = record.Prescriptions
            .Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                p.EndDate >= today, p.CreatedAt))
            .ToList();

        return new MedicalRecordDetailResponse(
            record.Id, record.AppointmentId, record.PetId, record.Pet.Name,
            record.VeterinarianId,
            record.Veterinarian.FirstName + " " + record.Veterinarian.LastName,
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate,
            record.CreatedAt, prescriptions);
    }

    public async Task<MedicalRecordResponse> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, ct)
            ?? throw new KeyNotFoundException($"Appointment with ID {request.AppointmentId} not found.");

        if (appointment.Status != AppointmentStatus.Completed && appointment.Status != AppointmentStatus.InProgress)
            throw new InvalidOperationException(
                $"Medical records can only be created for appointments with status 'Completed' or 'InProgress'. Current status: '{appointment.Status}'.");

        if (await db.MedicalRecords.AnyAsync(m => m.AppointmentId == request.AppointmentId, ct))
            throw new InvalidOperationException($"A medical record already exists for appointment {request.AppointmentId}.");

        var record = new MedicalRecord
        {
            AppointmentId = request.AppointmentId,
            PetId = appointment.PetId,
            VeterinarianId = appointment.VeterinarianId,
            Diagnosis = request.Diagnosis,
            Treatment = request.Treatment,
            Notes = request.Notes,
            FollowUpDate = request.FollowUpDate,
            CreatedAt = DateTime.UtcNow
        };

        db.MedicalRecords.Add(record);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created medical record {RecordId} for appointment {AppointmentId}",
            record.Id, record.AppointmentId);

        return new MedicalRecordResponse(
            record.Id, record.AppointmentId, record.PetId, appointment.Pet.Name,
            record.VeterinarianId,
            appointment.Veterinarian.FirstName + " " + appointment.Veterinarian.LastName,
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate,
            record.CreatedAt);
    }

    public async Task<MedicalRecordResponse?> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken ct)
    {
        var record = await db.MedicalRecords
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (record is null) return null;

        record.Diagnosis = request.Diagnosis;
        record.Treatment = request.Treatment;
        record.Notes = request.Notes;
        record.FollowUpDate = request.FollowUpDate;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated medical record {RecordId}", id);

        return new MedicalRecordResponse(
            record.Id, record.AppointmentId, record.PetId, record.Pet.Name,
            record.VeterinarianId,
            record.Veterinarian.FirstName + " " + record.Veterinarian.LastName,
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate,
            record.CreatedAt);
    }
}
