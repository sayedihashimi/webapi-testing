using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class OwnerEndpoints
{
    public static void MapOwnerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/owners").WithTags("Owners");

        group.MapGet("/", async Task<Ok<PaginatedResponse<OwnerResponse>>> (
            IOwnerService service,
            string? search,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);
            var result = await service.GetAllAsync(search, page, pageSize, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetOwners")
        .WithSummary("List all owners")
        .WithDescription("Returns a paginated list of owners. Supports search by name and email.")
        .Produces<PaginatedResponse<OwnerResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<OwnerDetailResponse>, NotFound>> (
            int id, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.GetByIdAsync(id, ct);
            return owner is null ? TypedResults.NotFound() : TypedResults.Ok(owner);
        })
        .WithName("GetOwnerById")
        .WithSummary("Get an owner by ID")
        .WithDescription("Returns owner details including their pets.")
        .Produces<OwnerDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<OwnerResponse>> (
            CreateOwnerRequest request, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/owners/{owner.Id}", owner);
        })
        .WithName("CreateOwner")
        .WithSummary("Create a new owner")
        .WithDescription("Creates a new pet owner. Email must be unique.")
        .Produces<OwnerResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<OwnerResponse>, NotFound>> (
            int id, UpdateOwnerRequest request, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.UpdateAsync(id, request, ct);
            return owner is null ? TypedResults.NotFound() : TypedResults.Ok(owner);
        })
        .WithName("UpdateOwner")
        .WithSummary("Update an owner")
        .WithDescription("Updates all fields of an existing owner.")
        .Produces<OwnerResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, IOwnerService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteOwner")
        .WithSummary("Delete an owner")
        .WithDescription("Deletes an owner. Fails if the owner has active pets.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/pets", async Task<Results<Ok<IReadOnlyList<PetResponse>>, NotFound>> (
            int id, IOwnerService service, CancellationToken ct) =>
        {
            var pets = await service.GetPetsAsync(id, ct);
            return TypedResults.Ok(pets);
        })
        .WithName("GetOwnerPets")
        .WithSummary("Get an owner's pets")
        .WithDescription("Returns all pets belonging to the specified owner.")
        .Produces<IReadOnlyList<PetResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/appointments", async Task<Results<Ok<PaginatedResponse<AppointmentResponse>>, NotFound>> (
            int id, IOwnerService service, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);
            var appointments = await service.GetAppointmentsAsync(id, page, pageSize, ct);
            return TypedResults.Ok(appointments);
        })
        .WithName("GetOwnerAppointments")
        .WithSummary("Get appointment history for an owner's pets")
        .WithDescription("Returns paginated appointment history for all pets of the specified owner.")
        .Produces<PaginatedResponse<AppointmentResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
