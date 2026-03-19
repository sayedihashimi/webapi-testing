using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class VaccinationService(VetClinicDbContext context, ILogger<VaccinationService> logger) : IVaccinationService
{
    public async Task<VaccinationResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var vaccination = await context.Vaccinations
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        if (vaccination is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new VaccinationResponse(
            vaccination.Id, vaccination.PetId, vaccination.Pet.Name,
            vaccination.VaccineName, vaccination.DateAdministered,
            vaccination.ExpirationDate, vaccination.BatchNumber,
            vaccination.AdministeredByVetId,
            $"{vaccination.AdministeredByVet.FirstName} {vaccination.AdministeredByVet.LastName}",
            vaccination.Notes,
            vaccination.ExpirationDate < today,
            vaccination.ExpirationDate >= today && vaccination.ExpirationDate <= today.AddDays(30),
            vaccination.CreatedAt);
    }

    public async Task<(VaccinationResponse? Result, string? Error)> CreateAsync(CreateVaccinationRequest request, CancellationToken cancellationToken)
    {
        if (request.ExpirationDate <= request.DateAdministered)
        {
            return (null, "ExpirationDate must be after DateAdministered.");
        }

        var petExists = await context.Pets.AnyAsync(p => p.Id == request.PetId, cancellationToken);
        if (!petExists)
        {
            return (null, "Pet not found.");
        }

        var vetExists = await context.Veterinarians.AnyAsync(v => v.Id == request.AdministeredByVetId, cancellationToken);
        if (!vetExists)
        {
            return (null, "Veterinarian not found.");
        }

        var vaccination = new Vaccination
        {
            PetId = request.PetId,
            VaccineName = request.VaccineName,
            DateAdministered = request.DateAdministered,
            ExpirationDate = request.ExpirationDate,
            BatchNumber = request.BatchNumber,
            AdministeredByVetId = request.AdministeredByVetId,
            Notes = request.Notes
        };

        context.Vaccinations.Add(vaccination);
        await context.SaveChangesAsync(cancellationToken);

        await context.Entry(vaccination).Reference(v => v.Pet).LoadAsync(cancellationToken);
        await context.Entry(vaccination).Reference(v => v.AdministeredByVet).LoadAsync(cancellationToken);

        logger.LogInformation("Vaccination recorded: {VaccinationId} for Pet {PetId}", vaccination.Id, vaccination.PetId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return (new VaccinationResponse(
            vaccination.Id, vaccination.PetId, vaccination.Pet.Name,
            vaccination.VaccineName, vaccination.DateAdministered,
            vaccination.ExpirationDate, vaccination.BatchNumber,
            vaccination.AdministeredByVetId,
            $"{vaccination.AdministeredByVet.FirstName} {vaccination.AdministeredByVet.LastName}",
            vaccination.Notes,
            vaccination.ExpirationDate < today,
            vaccination.ExpirationDate >= today && vaccination.ExpirationDate <= today.AddDays(30),
            vaccination.CreatedAt), null);
    }
}
