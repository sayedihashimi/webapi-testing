using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class OwnerEndpoints
{
    public static void MapOwnerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/owners").WithTags("Owners");

        group.MapGet("/", async (string? search, int page, int pageSize, IOwnerService service, CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;
            return Results.Ok(await service.GetAllAsync(search, page, pageSize, ct));
        })
        .WithName("GetOwners")
        .WithSummary("List all owners")
        .WithDescription("Returns a paginated list of owners. Supports search by name or email.")
        .Produces<PaginatedResponse<OwnerResponse>>();

        group.MapGet("/{id:int}", async (int id, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.GetByIdAsync(id, ct);
            return owner is null ? Results.NotFound() : Results.Ok(owner);
        })
        .WithName("GetOwnerById")
        .WithSummary("Get owner details")
        .WithDescription("Returns owner details including their pets.")
        .Produces<OwnerDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateOwnerRequest request, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.CreateAsync(request, ct);
            return Results.Created($"/api/owners/{owner.Id}", owner);
        })
        .WithName("CreateOwner")
        .WithSummary("Create a new owner")
        .WithDescription("Creates a new pet owner. Email must be unique.")
        .Produces<OwnerResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateOwnerRequest request, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.UpdateAsync(id, request, ct);
            return owner is null ? Results.NotFound() : Results.Ok(owner);
        })
        .WithName("UpdateOwner")
        .WithSummary("Update an owner")
        .WithDescription("Updates all fields for an existing owner.")
        .Produces<OwnerResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async (int id, IOwnerService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("DeleteOwner")
        .WithSummary("Delete an owner")
        .WithDescription("Deletes an owner. Fails if the owner has active pets.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/pets", async (int id, IOwnerService service, CancellationToken ct) =>
        {
            var pets = await service.GetOwnerPetsAsync(id, ct);
            return pets is null ? Results.NotFound() : Results.Ok(pets);
        })
        .WithName("GetOwnerPets")
        .WithSummary("Get owner's pets")
        .WithDescription("Returns all pets belonging to the specified owner.")
        .Produces<List<PetResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/appointments", async (int id, int page, int pageSize, IOwnerService service, CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;
            var result = await service.GetOwnerAppointmentsAsync(id, page, pageSize, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetOwnerAppointments")
        .WithSummary("Get owner's appointment history")
        .WithDescription("Returns paginated appointment history for all of the owner's pets.")
        .Produces<PaginatedResponse<AppointmentResponse>>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
