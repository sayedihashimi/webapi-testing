using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PetEndpoints
{
    public static RouteGroupBuilder MapPetEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/pets").WithTags("Pets");

        group.MapGet("/", GetAll)
            .WithName("GetPets")
            .WithSummary("Get all pets with optional search, species filter, and pagination");

        group.MapGet("/{id:int}", GetById)
            .WithName("GetPetById")
            .WithSummary("Get pet by ID with owner info");

        group.MapPost("/", Create)
            .WithName("CreatePet")
            .WithSummary("Create a new pet");

        group.MapPut("/{id:int}", Update)
            .WithName("UpdatePet")
            .WithSummary("Update a pet (including owner transfer)");

        group.MapDelete("/{id:int}", SoftDelete)
            .WithName("DeletePet")
            .WithSummary("Soft-delete a pet (sets IsActive to false)");

        group.MapGet("/{id:int}/medical-records", GetMedicalRecords)
            .WithName("GetPetMedicalRecords")
            .WithSummary("Get all medical records for a pet");

        group.MapGet("/{id:int}/vaccinations", GetVaccinations)
            .WithName("GetPetVaccinations")
            .WithSummary("Get all vaccinations for a pet");

        group.MapGet("/{id:int}/vaccinations/upcoming", GetUpcomingVaccinations)
            .WithName("GetPetUpcomingVaccinations")
            .WithSummary("Get vaccinations that are due soon or overdue");

        group.MapGet("/{id:int}/prescriptions/active", GetActivePrescriptions)
            .WithName("GetPetActivePrescriptions")
            .WithSummary("Get active prescriptions for a pet");

        return group;
    }

    private static async Task<Ok<PagedResponse<PetResponse>>> GetAll(
        PetService service, string? search = null, string? species = null,
        bool includeInactive = false, int page = 1, int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var result = await service.GetAllAsync(search, species, includeInactive, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<PetDetailResponse>, NotFound>> GetById(
        int id, PetService service, CancellationToken ct)
    {
        var pet = await service.GetByIdAsync(id, ct);
        return pet is not null ? TypedResults.Ok(pet) : TypedResults.NotFound();
    }

    private static async Task<Created<PetResponse>> Create(
        CreatePetRequest request, PetService service, CancellationToken ct)
    {
        var pet = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/pets/{pet.Id}", pet);
    }

    private static async Task<Results<Ok<PetResponse>, NotFound>> Update(
        int id, UpdatePetRequest request, PetService service, CancellationToken ct)
    {
        var pet = await service.UpdateAsync(id, request, ct);
        return pet is not null ? TypedResults.Ok(pet) : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound>> SoftDelete(
        int id, PetService service, CancellationToken ct)
    {
        var deleted = await service.SoftDeleteAsync(id, ct);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    private static async Task<Ok<List<MedicalRecordResponse>>> GetMedicalRecords(
        int id, PetService service, CancellationToken ct)
    {
        var records = await service.GetMedicalRecordsAsync(id, ct);
        return TypedResults.Ok(records);
    }

    private static async Task<Ok<List<VaccinationResponse>>> GetVaccinations(
        int id, PetService service, CancellationToken ct)
    {
        var vaccinations = await service.GetVaccinationsAsync(id, ct);
        return TypedResults.Ok(vaccinations);
    }

    private static async Task<Ok<List<VaccinationResponse>>> GetUpcomingVaccinations(
        int id, PetService service, CancellationToken ct)
    {
        var vaccinations = await service.GetUpcomingVaccinationsAsync(id, ct);
        return TypedResults.Ok(vaccinations);
    }

    private static async Task<Ok<List<PrescriptionResponse>>> GetActivePrescriptions(
        int id, PetService service, CancellationToken ct)
    {
        var prescriptions = await service.GetActivePrescriptionsAsync(id, ct);
        return TypedResults.Ok(prescriptions);
    }
}
