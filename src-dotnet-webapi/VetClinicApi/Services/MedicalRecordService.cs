using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IMedicalRecordService
{
    Task<MedicalRecordDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<MedicalRecordResponse> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct);
    Task<MedicalRecordResponse?> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken ct);
}

public class MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger) : IMedicalRecordService
{
    public async Task<MedicalRecordDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await db.MedicalRecords.AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .Where(m => m.Id == id)
            .Select(m => new MedicalRecordDetailResponse(
                m.Id, m.AppointmentId, m.PetId, m.Pet.Name,
                m.VeterinarianId, $"{m.Veterinarian.FirstName} {m.Veterinarian.LastName}",
                m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt,
                m.Prescriptions.Select(p => new PrescriptionResponse(
                    p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage, p.DurationDays,
                    p.StartDate, p.EndDate, p.Instructions, p.EndDate >= today, p.CreatedAt)).ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<MedicalRecordResponse> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, ct)
            ?? throw new KeyNotFoundException($"Appointment with ID {request.AppointmentId} not found.");

        if (appointment.Status is not (AppointmentStatus.Completed or AppointmentStatus.InProgress))
            throw new InvalidOperationException("Medical records can only be created for appointments with status InProgress or Completed.");

        if (await db.MedicalRecords.AnyAsync(m => m.AppointmentId == request.AppointmentId, ct))
            throw new InvalidOperationException($"A medical record already exists for appointment {request.AppointmentId}.");

        var record = new MedicalRecord
        {
            AppointmentId = appointment.Id,
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
        logger.LogInformation("Created medical record {RecordId} for appointment {AppointmentId}", record.Id, record.AppointmentId);

        return new MedicalRecordResponse(record.Id, record.AppointmentId, record.PetId, appointment.Pet.Name,
            record.VeterinarianId, $"{appointment.Veterinarian.FirstName} {appointment.Veterinarian.LastName}",
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate, record.CreatedAt);
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

        return new MedicalRecordResponse(record.Id, record.AppointmentId, record.PetId, record.Pet.Name,
            record.VeterinarianId, $"{record.Veterinarian.FirstName} {record.Veterinarian.LastName}",
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate, record.CreatedAt);
    }
}
