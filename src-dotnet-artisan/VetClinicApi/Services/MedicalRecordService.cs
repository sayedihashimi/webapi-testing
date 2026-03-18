using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger)
{
    public async Task<MedicalRecordResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var record = await db.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        return record is null ? null : PetService.MapToMedicalRecordResponse(record);
    }

    public async Task<MedicalRecordResponse> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, ct)
            ?? throw new KeyNotFoundException($"Appointment with ID {request.AppointmentId} not found.");

        if (appointment.Status is not (AppointmentStatus.Completed or AppointmentStatus.InProgress))
        {
            throw new InvalidOperationException(
                $"Medical records can only be created for appointments with status 'Completed' or 'InProgress'. Current status: '{appointment.Status}'.");
        }

        if (await db.MedicalRecords.AnyAsync(m => m.AppointmentId == request.AppointmentId, ct))
        {
            throw new InvalidOperationException($"A medical record already exists for appointment {request.AppointmentId}.");
        }

        var record = new MedicalRecord
        {
            AppointmentId = request.AppointmentId,
            PetId = appointment.PetId,
            VeterinarianId = appointment.VeterinarianId,
            Diagnosis = request.Diagnosis,
            Treatment = request.Treatment,
            Notes = request.Notes,
            FollowUpDate = request.FollowUpDate
        };

        db.MedicalRecords.Add(record);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created medical record {RecordId} for appointment {AppointmentId}", record.Id, request.AppointmentId);

        // Reload with navigation properties
        var created = await db.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstAsync(m => m.Id == record.Id, ct);

        return PetService.MapToMedicalRecordResponse(created);
    }

    public async Task<MedicalRecordResponse?> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken ct)
    {
        var record = await db.MedicalRecords.FindAsync([id], ct);
        if (record is null)
        {
            return null;
        }

        record.Diagnosis = request.Diagnosis;
        record.Treatment = request.Treatment;
        record.Notes = request.Notes;
        record.FollowUpDate = request.FollowUpDate;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated medical record {RecordId}", id);

        var updated = await db.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstAsync(m => m.Id == id, ct);

        return PetService.MapToMedicalRecordResponse(updated);
    }
}
