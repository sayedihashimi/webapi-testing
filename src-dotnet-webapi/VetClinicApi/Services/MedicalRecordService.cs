using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger) : IMedicalRecordService
{
    public async Task<MedicalRecordResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var record = await db.MedicalRecords.AsNoTracking()
            .Include(mr => mr.Pet)
            .Include(mr => mr.Veterinarian)
            .Include(mr => mr.Prescriptions)
            .FirstOrDefaultAsync(mr => mr.Id == id, ct);

        return record is null ? null : MapToResponse(record);
    }

    public async Task<MedicalRecordResponse> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, ct);

        if (appointment is null)
            throw new KeyNotFoundException($"Appointment with ID {request.AppointmentId} not found.");

        if (appointment.Status is not (AppointmentStatus.Completed or AppointmentStatus.InProgress))
            throw new ArgumentException($"Medical records can only be created for appointments with status 'Completed' or 'InProgress'. Current status: '{appointment.Status}'.");

        var existingRecord = await db.MedicalRecords.AsNoTracking().AnyAsync(mr => mr.AppointmentId == request.AppointmentId, ct);
        if (existingRecord)
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

        var created = await db.MedicalRecords.AsNoTracking()
            .Include(mr => mr.Pet)
            .Include(mr => mr.Veterinarian)
            .Include(mr => mr.Prescriptions)
            .FirstAsync(mr => mr.Id == record.Id, ct);

        logger.LogInformation("Created medical record {RecordId} for appointment {AppointmentId}", record.Id, record.AppointmentId);

        return MapToResponse(created);
    }

    public async Task<MedicalRecordResponse?> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken ct)
    {
        var record = await db.MedicalRecords
            .Include(mr => mr.Pet)
            .Include(mr => mr.Veterinarian)
            .Include(mr => mr.Prescriptions)
            .FirstOrDefaultAsync(mr => mr.Id == id, ct);

        if (record is null) return null;

        record.Diagnosis = request.Diagnosis;
        record.Treatment = request.Treatment;
        record.Notes = request.Notes;
        record.FollowUpDate = request.FollowUpDate;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated medical record {RecordId}", id);

        return MapToResponse(record);
    }

    private static MedicalRecordResponse MapToResponse(MedicalRecord mr) => new()
    {
        Id = mr.Id,
        AppointmentId = mr.AppointmentId,
        PetId = mr.PetId,
        VeterinarianId = mr.VeterinarianId,
        Diagnosis = mr.Diagnosis,
        Treatment = mr.Treatment,
        Notes = mr.Notes,
        FollowUpDate = mr.FollowUpDate,
        CreatedAt = mr.CreatedAt,
        Pet = mr.Pet != null ? new PetSummaryResponse
        {
            Id = mr.Pet.Id,
            Name = mr.Pet.Name,
            Species = mr.Pet.Species,
            Breed = mr.Pet.Breed,
            IsActive = mr.Pet.IsActive
        } : null,
        Veterinarian = mr.Veterinarian != null ? new VeterinarianSummaryResponse
        {
            Id = mr.Veterinarian.Id,
            FirstName = mr.Veterinarian.FirstName,
            LastName = mr.Veterinarian.LastName,
            Specialization = mr.Veterinarian.Specialization
        } : null,
        Prescriptions = mr.Prescriptions.Select(p => new PrescriptionResponse
        {
            Id = p.Id,
            MedicalRecordId = p.MedicalRecordId,
            MedicationName = p.MedicationName,
            Dosage = p.Dosage,
            DurationDays = p.DurationDays,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Instructions = p.Instructions,
            IsActive = p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = p.CreatedAt
        }).ToList()
    };
}
