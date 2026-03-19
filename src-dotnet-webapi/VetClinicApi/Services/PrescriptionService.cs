using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PrescriptionService(VetClinicDbContext db, ILogger<PrescriptionService> logger)
    : IPrescriptionService
{
    public async Task<PrescriptionResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var prescription = await db.Prescriptions.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (prescription is null) return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new PrescriptionResponse(
            prescription.Id, prescription.MedicalRecordId,
            prescription.MedicationName, prescription.Dosage,
            prescription.DurationDays, prescription.StartDate, prescription.EndDate,
            prescription.Instructions, prescription.EndDate >= today,
            prescription.CreatedAt);
    }

    public async Task<PrescriptionResponse> CreateAsync(CreatePrescriptionRequest request, CancellationToken ct)
    {
        if (!await db.MedicalRecords.AnyAsync(m => m.Id == request.MedicalRecordId, ct))
            throw new KeyNotFoundException($"Medical record with ID {request.MedicalRecordId} not found.");

        var endDate = request.StartDate.AddDays(request.DurationDays);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var prescription = new Prescription
        {
            MedicalRecordId = request.MedicalRecordId,
            MedicationName = request.MedicationName,
            Dosage = request.Dosage,
            DurationDays = request.DurationDays,
            StartDate = request.StartDate,
            EndDate = endDate,
            Instructions = request.Instructions,
            CreatedAt = DateTime.UtcNow
        };

        db.Prescriptions.Add(prescription);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created prescription {PrescriptionId} for medical record {RecordId}",
            prescription.Id, prescription.MedicalRecordId);

        return new PrescriptionResponse(
            prescription.Id, prescription.MedicalRecordId,
            prescription.MedicationName, prescription.Dosage,
            prescription.DurationDays, prescription.StartDate, prescription.EndDate,
            prescription.Instructions, prescription.EndDate >= today,
            prescription.CreatedAt);
    }
}
