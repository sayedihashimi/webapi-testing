using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PetEndpoints
{
    public static void MapPetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pets").WithTags("Pets");

        group.MapGet("/", async Task<Ok<PaginatedResponse<PetResponse>>> (
            IPetService service,
            string? search,
            string? species,
            bool includeInactive = false,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);
            var result = await service.GetAllAsync(search, species, includeInactive, page, pageSize, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPets")
        .WithSummary("List all pets")
        .WithDescription("Returns a paginated list of active pets. Supports search by name and filter by species. Use includeInactive=true to include soft-deleted pets.")
        .Produces<PaginatedResponse<PetResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<PetDetailResponse>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.GetByIdAsync(id, ct);
            return pet is null ? TypedResults.NotFound() : TypedResults.Ok(pet);
        })
        .WithName("GetPetById")
        .WithSummary("Get a pet by ID")
        .WithDescription("Returns pet details including owner information.")
        .Produces<PetDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<PetResponse>> (
            CreatePetRequest request, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/pets/{pet.Id}", pet);
        })
        .WithName("CreatePet")
        .WithSummary("Create a new pet")
        .WithDescription("Creates a new pet. Valid species: Dog, Cat, Bird, Rabbit. Microchip number must be unique.")
        .Produces<PetResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async Task<Results<Ok<PetResponse>, NotFound>> (
            int id, UpdatePetRequest request, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.UpdateAsync(id, request, ct);
            return pet is null ? TypedResults.NotFound() : TypedResults.Ok(pet);
        })
        .WithName("UpdatePet")
        .WithSummary("Update a pet")
        .WithDescription("Updates all fields of an existing pet. Change OwnerId to transfer ownership.")
        .Produces<PetResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            await service.SoftDeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeletePet")
        .WithSummary("Soft delete a pet")
        .WithDescription("Marks a pet as inactive (soft delete). The pet record is preserved.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/medical-records", async Task<Results<Ok<IReadOnlyList<MedicalRecordResponse>>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var records = await service.GetMedicalRecordsAsync(id, ct);
            return TypedResults.Ok(records);
        })
        .WithName("GetPetMedicalRecords")
        .WithSummary("Get medical records for a pet")
        .WithDescription("Returns all medical records for the specified pet.")
        .Produces<IReadOnlyList<MedicalRecordResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/vaccinations", async Task<Results<Ok<IReadOnlyList<VaccinationResponse>>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var vaccinations = await service.GetVaccinationsAsync(id, ct);
            return TypedResults.Ok(vaccinations);
        })
        .WithName("GetPetVaccinations")
        .WithSummary("Get vaccinations for a pet")
        .WithDescription("Returns all vaccination records for the specified pet.")
        .Produces<IReadOnlyList<VaccinationResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/vaccinations/upcoming", async Task<Results<Ok<IReadOnlyList<VaccinationResponse>>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var vaccinations = await service.GetUpcomingVaccinationsAsync(id, ct);
            return TypedResults.Ok(vaccinations);
        })
        .WithName("GetPetUpcomingVaccinations")
        .WithSummary("Get upcoming and overdue vaccinations for a pet")
        .WithDescription("Returns vaccinations that are due soon (within 30 days) or already expired.")
        .Produces<IReadOnlyList<VaccinationResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/prescriptions/active", async Task<Results<Ok<IReadOnlyList<PrescriptionResponse>>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var prescriptions = await service.GetActivePrescriptionsAsync(id, ct);
            return TypedResults.Ok(prescriptions);
        })
        .WithName("GetPetActivePrescriptions")
        .WithSummary("Get active prescriptions for a pet")
        .WithDescription("Returns all currently active prescriptions for the specified pet.")
        .Produces<IReadOnlyList<PrescriptionResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
