using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class MedicalRecordService(VetClinicDbContext context, ILogger<MedicalRecordService> logger) : IMedicalRecordService
{
    public async Task<MedicalRecordDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var record = await context.MedicalRecords
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (record is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var prescriptions = record.Prescriptions.Select(p => new PrescriptionResponse(
            p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
            p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
            p.EndDate >= today, p.CreatedAt)).ToList();

        return new MedicalRecordDetailResponse(
            record.Id, record.AppointmentId, record.PetId, record.Pet.Name,
            record.VeterinarianId, $"{record.Veterinarian.FirstName} {record.Veterinarian.LastName}",
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate,
            record.CreatedAt, prescriptions);
    }

    public async Task<(MedicalRecordResponse? Result, string? Error)> CreateAsync(CreateMedicalRecordRequest request, CancellationToken cancellationToken)
    {
        var appointment = await context.Appointments.FindAsync([request.AppointmentId], cancellationToken);
        if (appointment is null)
        {
            return (null, "Appointment not found.");
        }

        if (appointment.Status is not (AppointmentStatus.Completed or AppointmentStatus.InProgress))
        {
            return (null, $"Medical records can only be created for appointments with status 'Completed' or 'InProgress'. Current status: '{appointment.Status}'.");
        }

        var existingRecord = await context.MedicalRecords
            .AnyAsync(m => m.AppointmentId == request.AppointmentId, cancellationToken);
        if (existingRecord)
        {
            return (null, "A medical record already exists for this appointment.");
        }

        var record = new MedicalRecord
        {
            AppointmentId = request.AppointmentId,
            PetId = request.PetId,
            VeterinarianId = request.VeterinarianId,
            Diagnosis = request.Diagnosis,
            Treatment = request.Treatment,
            Notes = request.Notes,
            FollowUpDate = request.FollowUpDate
        };

        context.MedicalRecords.Add(record);
        await context.SaveChangesAsync(cancellationToken);

        await context.Entry(record).Reference(r => r.Pet).LoadAsync(cancellationToken);
        await context.Entry(record).Reference(r => r.Veterinarian).LoadAsync(cancellationToken);

        logger.LogInformation("Medical record created: {RecordId} for Appointment {AppointmentId}", record.Id, record.AppointmentId);

        return (new MedicalRecordResponse(
            record.Id, record.AppointmentId, record.PetId, record.Pet.Name,
            record.VeterinarianId, $"{record.Veterinarian.FirstName} {record.Veterinarian.LastName}",
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate,
            record.CreatedAt), null);
    }

    public async Task<(MedicalRecordResponse? Result, string? Error, bool NotFound)> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken cancellationToken)
    {
        var record = await context.MedicalRecords
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (record is null)
        {
            return (null, null, true);
        }

        record.Diagnosis = request.Diagnosis;
        record.Treatment = request.Treatment;
        record.Notes = request.Notes;
        record.FollowUpDate = request.FollowUpDate;

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Medical record updated: {RecordId}", record.Id);

        return (new MedicalRecordResponse(
            record.Id, record.AppointmentId, record.PetId, record.Pet.Name,
            record.VeterinarianId, $"{record.Veterinarian.FirstName} {record.Veterinarian.LastName}",
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate,
            record.CreatedAt), null, false);
    }
}
