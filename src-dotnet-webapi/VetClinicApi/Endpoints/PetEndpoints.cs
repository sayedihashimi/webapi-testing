using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PetEndpoints
{
    public static RouteGroupBuilder MapPetEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/pets").WithTags("Pets");

        group.MapGet("/", async (string? search, string? species, bool? includeInactive, int page, int pageSize, IPetService service, CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;
            return Results.Ok(await service.GetAllAsync(search, species, includeInactive ?? false, page, pageSize, ct));
        })
        .WithName("GetPets")
        .WithSummary("List pets")
        .WithDescription("Returns a paginated list of pets. By default excludes inactive pets. Use includeInactive=true to include them.")
        .Produces<PagedResponse<PetResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async (int id, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.GetByIdAsync(id, ct);
            return pet is null ? Results.NotFound() : Results.Ok(pet);
        })
        .WithName("GetPetById")
        .WithSummary("Get a pet by ID")
        .WithDescription("Returns pet details including owner information.")
        .Produces<PetResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreatePetRequest request, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/pets/{pet.Id}", pet);
        })
        .WithName("CreatePet")
        .WithSummary("Create a new pet")
        .WithDescription("Registers a new pet. Species must be Dog, Cat, Bird, or Rabbit.")
        .Produces<PetResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:int}", async (int id, UpdatePetRequest request, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.UpdateAsync(id, request, ct);
            return pet is null ? Results.NotFound() : Results.Ok(pet);
        })
        .WithName("UpdatePet")
        .WithSummary("Update a pet")
        .WithDescription("Updates pet information. Changing OwnerId transfers ownership.")
        .Produces<PetResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:int}", async (int id, IPetService service, CancellationToken ct) =>
        {
            await service.SoftDeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("DeletePet")
        .WithSummary("Soft delete a pet")
        .WithDescription("Sets the pet's IsActive flag to false (soft delete).")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/medical-records", async (int id, int page, int pageSize, IPetService service, CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;
            return Results.Ok(await service.GetMedicalRecordsAsync(id, page, pageSize, ct));
        })
        .WithName("GetPetMedicalRecords")
        .WithSummary("Get a pet's medical records")
        .WithDescription("Returns all medical records for the specified pet, including prescriptions.")
        .Produces<PagedResponse<MedicalRecordResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/vaccinations", async (int id, IPetService service, CancellationToken ct) =>
        {
            return Results.Ok(await service.GetVaccinationsAsync(id, ct));
        })
        .WithName("GetPetVaccinations")
        .WithSummary("Get a pet's vaccinations")
        .WithDescription("Returns all vaccination records for the specified pet.")
        .Produces<List<VaccinationResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/vaccinations/upcoming", async (int id, IPetService service, CancellationToken ct) =>
        {
            return Results.Ok(await service.GetUpcomingVaccinationsAsync(id, ct));
        })
        .WithName("GetPetUpcomingVaccinations")
        .WithSummary("Get upcoming and overdue vaccinations")
        .WithDescription("Returns vaccinations that are due soon (within 30 days) or already expired.")
        .Produces<List<VaccinationResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/prescriptions/active", async (int id, IPetService service, CancellationToken ct) =>
        {
            return Results.Ok(await service.GetActivePrescriptionsAsync(id, ct));
        })
        .WithName("GetPetActivePrescriptions")
        .WithSummary("Get active prescriptions for a pet")
        .WithDescription("Returns all currently active prescriptions for the specified pet.")
        .Produces<List<PrescriptionResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
