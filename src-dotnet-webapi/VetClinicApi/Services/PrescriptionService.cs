using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PrescriptionService(VetClinicDbContext db, ILogger<PrescriptionService> logger) : IPrescriptionService
{
    public async Task<PrescriptionResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var prescription = await db.Prescriptions.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return prescription is null ? null : MapToResponse(prescription);
    }

    public async Task<PrescriptionResponse> CreateAsync(CreatePrescriptionRequest request, CancellationToken ct)
    {
        var recordExists = await db.MedicalRecords.AsNoTracking().AnyAsync(mr => mr.Id == request.MedicalRecordId, ct);
        if (!recordExists)
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
        logger.LogInformation("Created prescription {PrescriptionId} for medical record {RecordId}", prescription.Id, prescription.MedicalRecordId);

        return MapToResponse(prescription);
    }

    private static PrescriptionResponse MapToResponse(Prescription p) => new()
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
    };
}
