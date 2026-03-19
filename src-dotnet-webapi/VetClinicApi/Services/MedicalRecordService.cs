using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger) : IMedicalRecordService
{
    public async Task<MedicalRecordResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var record = await db.MedicalRecords.AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        return record is null ? null : MapToResponse(record);
    }

    public async Task<MedicalRecordResponse> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, ct)
            ?? throw new KeyNotFoundException($"Appointment with ID {request.AppointmentId} not found.");

        if (appointment.Status is not (AppointmentStatus.Completed or AppointmentStatus.InProgress))
            throw new ArgumentException("Medical records can only be created for Completed or InProgress appointments.");

        if (await db.MedicalRecords.AnyAsync(m => m.AppointmentId == request.AppointmentId, ct))
            throw new InvalidOperationException("A medical record already exists for this appointment.");

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

        var created = await db.MedicalRecords.AsNoTracking()
            .Include(m => m.Pet).Include(m => m.Veterinarian).Include(m => m.Prescriptions)
            .FirstAsync(m => m.Id == record.Id, ct);

        logger.LogInformation("Created medical record {RecordId} for appointment {AppointmentId}", record.Id, request.AppointmentId);
        return MapToResponse(created);
    }

    public async Task<MedicalRecordResponse?> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken ct)
    {
        var record = await db.MedicalRecords.FindAsync([id], ct);
        if (record is null) return null;

        record.Diagnosis = request.Diagnosis;
        record.Treatment = request.Treatment;
        record.Notes = request.Notes;
        record.FollowUpDate = request.FollowUpDate;

        await db.SaveChangesAsync(ct);

        var updated = await db.MedicalRecords.AsNoTracking()
            .Include(m => m.Pet).Include(m => m.Veterinarian).Include(m => m.Prescriptions)
            .FirstAsync(m => m.Id == id, ct);

        logger.LogInformation("Updated medical record {RecordId}", id);
        return MapToResponse(updated);
    }

    private static MedicalRecordResponse MapToResponse(MedicalRecord m)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new MedicalRecordResponse(
            m.Id, m.AppointmentId, m.PetId, m.Pet.Name, m.VeterinarianId,
            $"{m.Veterinarian.FirstName} {m.Veterinarian.LastName}",
            m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt,
            m.Prescriptions.Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage, p.DurationDays,
                p.StartDate, p.EndDate, p.Instructions, p.EndDate >= today, p.CreatedAt)).ToList());
    }
}
