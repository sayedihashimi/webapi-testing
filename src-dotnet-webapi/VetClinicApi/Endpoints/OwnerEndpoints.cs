using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class OwnerEndpoints
{
    public static RouteGroupBuilder MapOwnerEndpoints(this WebApplication app)
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
        .WithDescription("Returns a paginated list of owners with optional search by name or email.")
        .Produces<PagedResponse<OwnerSummaryResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async (int id, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.GetByIdAsync(id, ct);
            return owner is null ? Results.NotFound() : Results.Ok(owner);
        })
        .WithName("GetOwnerById")
        .WithSummary("Get an owner by ID")
        .WithDescription("Returns owner details including their pets.")
        .Produces<OwnerResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateOwnerRequest request, IOwnerService service, CancellationToken ct) =>
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

        group.MapPut("/{id:int}", async (int id, UpdateOwnerRequest request, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.UpdateAsync(id, request, ct);
            return owner is null ? Results.NotFound() : Results.Ok(owner);
        })
        .WithName("UpdateOwner")
        .WithSummary("Update an owner")
        .WithDescription("Updates an existing owner's information.")
        .Produces<OwnerResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

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
            return Results.Ok(await service.GetOwnerPetsAsync(id, ct));
        })
        .WithName("GetOwnerPets")
        .WithSummary("Get an owner's pets")
        .WithDescription("Returns all pets belonging to the specified owner.")
        .Produces<PagedResponse<PetSummaryResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/appointments", async (int id, int page, int pageSize, IOwnerService service, CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;
            return Results.Ok(await service.GetOwnerAppointmentsAsync(id, page, pageSize, ct));
        })
        .WithName("GetOwnerAppointments")
        .WithSummary("Get an owner's appointment history")
        .WithDescription("Returns appointment history for all of the owner's pets.")
        .Produces<PagedResponse<AppointmentResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
