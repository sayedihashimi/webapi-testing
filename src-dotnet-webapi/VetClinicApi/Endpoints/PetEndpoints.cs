using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PetEndpoints
{
    public static RouteGroupBuilder MapPetEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/pets").WithTags("Pets");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<PetResponse>>, BadRequest>> (
            string? search, string? species, bool? includeInactive, int? page, int? pageSize,
            IPetService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(search, species, includeInactive ?? false, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPets")
        .WithSummary("List all active pets")
        .WithDescription("Returns a paginated list of pets. Filter by name, species. Set includeInactive=true to see soft-deleted pets.");

        group.MapGet("/{id:int}", async Task<Results<Ok<PetResponse>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.GetByIdAsync(id, ct);
            return pet is null ? TypedResults.NotFound() : TypedResults.Ok(pet);
        })
        .WithName("GetPetById")
        .WithSummary("Get pet by ID")
        .WithDescription("Returns pet details including owner information.");

        group.MapPost("/", async Task<Results<Created<PetResponse>, BadRequest>> (
            CreatePetRequest request, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/pets/{pet.Id}", pet);
        })
        .WithName("CreatePet")
        .WithSummary("Create a new pet")
        .WithDescription("Registers a new pet. Owner must exist. Microchip number must be unique.");

        group.MapPut("/{id:int}", async Task<Results<Ok<PetResponse>, NotFound>> (
            int id, UpdatePetRequest request, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.UpdateAsync(id, request, ct);
            return pet is null ? TypedResults.NotFound() : TypedResults.Ok(pet);
        })
        .WithName("UpdatePet")
        .WithSummary("Update a pet")
        .WithDescription("Updates pet information. Changing OwnerId transfers ownership.");

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var deleted = await service.SoftDeleteAsync(id, ct);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeletePet")
        .WithSummary("Soft delete a pet")
        .WithDescription("Marks the pet as inactive (soft delete).");

        group.MapGet("/{id:int}/medical-records", async Task<Results<Ok<IReadOnlyList<MedicalRecordResponse>>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var records = await service.GetMedicalRecordsAsync(id, ct);
            return TypedResults.Ok(records);
        })
        .WithName("GetPetMedicalRecords")
        .WithSummary("Get pet's medical records")
        .WithDescription("Returns all medical records for the specified pet.");

        group.MapGet("/{id:int}/vaccinations", async Task<Results<Ok<IReadOnlyList<VaccinationResponse>>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var vaccinations = await service.GetVaccinationsAsync(id, ct);
            return TypedResults.Ok(vaccinations);
        })
        .WithName("GetPetVaccinations")
        .WithSummary("Get pet's vaccinations")
        .WithDescription("Returns all vaccination records for the specified pet.");

        group.MapGet("/{id:int}/vaccinations/upcoming", async Task<Results<Ok<IReadOnlyList<VaccinationResponse>>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var vaccinations = await service.GetUpcomingVaccinationsAsync(id, ct);
            return TypedResults.Ok(vaccinations);
        })
        .WithName("GetPetUpcomingVaccinations")
        .WithSummary("Get pet's upcoming or overdue vaccinations")
        .WithDescription("Returns vaccinations that are due soon (within 30 days) or already expired.");

        group.MapGet("/{id:int}/prescriptions/active", async Task<Results<Ok<IReadOnlyList<PrescriptionResponse>>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var prescriptions = await service.GetActivePrescriptionsAsync(id, ct);
            return TypedResults.Ok(prescriptions);
        })
        .WithName("GetPetActivePrescriptions")
        .WithSummary("Get pet's active prescriptions")
        .WithDescription("Returns currently active prescriptions (EndDate >= today) for the specified pet.");

        return group;
    }
}
