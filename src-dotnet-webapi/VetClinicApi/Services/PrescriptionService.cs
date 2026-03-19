using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PrescriptionService(VetClinicDbContext db, ILogger<PrescriptionService> logger) : IPrescriptionService
{
    public async Task<PrescriptionResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var p = await db.Prescriptions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (p is null) return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new PrescriptionResponse(
            p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage, p.DurationDays,
            p.StartDate, p.EndDate, p.Instructions, p.EndDate >= today, p.CreatedAt);
    }

    public async Task<PrescriptionResponse> CreateAsync(CreatePrescriptionRequest request, CancellationToken ct)
    {
        if (!await db.MedicalRecords.AnyAsync(m => m.Id == request.MedicalRecordId, ct))
            throw new KeyNotFoundException($"Medical record with ID {request.MedicalRecordId} not found.");

        var prescription = new Prescription
        {
            MedicalRecordId = request.MedicalRecordId,
            MedicationName = request.MedicationName,
            Dosage = request.Dosage,
            DurationDays = request.DurationDays,
            StartDate = request.StartDate,
            EndDate = request.StartDate.AddDays(request.DurationDays),
            Instructions = request.Instructions,
            CreatedAt = DateTime.UtcNow
        };

        db.Prescriptions.Add(prescription);
        await db.SaveChangesAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        logger.LogInformation("Created prescription {PrescriptionId} for record {RecordId}", prescription.Id, request.MedicalRecordId);

        return new PrescriptionResponse(
            prescription.Id, prescription.MedicalRecordId, prescription.MedicationName,
            prescription.Dosage, prescription.DurationDays, prescription.StartDate,
            prescription.EndDate, prescription.Instructions, prescription.EndDate >= today,
            prescription.CreatedAt);
    }
}
