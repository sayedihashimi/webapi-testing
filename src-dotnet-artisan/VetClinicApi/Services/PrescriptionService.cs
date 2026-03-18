using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PrescriptionService(VetClinicDbContext db, ILogger<PrescriptionService> logger)
{
    public async Task<PrescriptionResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var rx = await db.Prescriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (rx is null)
        {
            return null;
        }

        return new PrescriptionResponse(
            rx.Id, rx.MedicalRecordId, rx.MedicationName, rx.Dosage,
            rx.DurationDays, rx.StartDate, rx.EndDate, rx.IsActive,
            rx.Instructions, rx.CreatedAt);
    }

    public async Task<PrescriptionResponse> CreateAsync(CreatePrescriptionRequest request, CancellationToken ct)
    {
        if (!await db.MedicalRecords.AnyAsync(m => m.Id == request.MedicalRecordId, ct))
        {
            throw new KeyNotFoundException($"Medical record with ID {request.MedicalRecordId} not found.");
        }

        var rx = new Prescription
        {
            MedicalRecordId = request.MedicalRecordId,
            MedicationName = request.MedicationName,
            Dosage = request.Dosage,
            DurationDays = request.DurationDays,
            StartDate = request.StartDate,
            Instructions = request.Instructions
        };

        db.Prescriptions.Add(rx);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created prescription {PrescriptionId} for medical record {RecordId}",
            rx.Id, request.MedicalRecordId);

        return new PrescriptionResponse(
            rx.Id, rx.MedicalRecordId, rx.MedicationName, rx.Dosage,
            rx.DurationDays, rx.StartDate, rx.EndDate, rx.IsActive,
            rx.Instructions, rx.CreatedAt);
    }
}
