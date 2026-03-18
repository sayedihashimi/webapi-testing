using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class ClassTypeEndpoints
{
    public static void MapClassTypeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/class-types").WithTags("Class Types");

        group.MapGet("/", async (string? difficulty, bool? isPremium, int? page, int? pageSize, IClassTypeService service, CancellationToken ct) =>
        {
            var p = Math.Max(1, page ?? 1);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            return Results.Ok(await service.GetAllAsync(difficulty, isPremium, p, ps, ct));
        })
        .WithName("GetClassTypes")
        .WithSummary("List class types")
        .WithDescription("Returns a paginated list of active class types with optional difficulty and premium filters.")
        .Produces<PaginatedResponse<ClassTypeResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async (int id, IClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.GetByIdAsync(id, ct);
            return classType is null ? Results.NotFound() : Results.Ok(classType);
        })
        .WithName("GetClassTypeById")
        .WithSummary("Get class type details")
        .WithDescription("Returns the details of a specific class type.")
        .Produces<ClassTypeResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateClassTypeRequest request, IClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.CreateAsync(request, ct);
            return Results.Created($"/api/class-types/{classType.Id}", classType);
        })
        .WithName("CreateClassType")
        .WithSummary("Create a new class type")
        .WithDescription("Creates a new class type for the studio.")
        .Produces<ClassTypeResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateClassTypeRequest request, IClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.UpdateAsync(id, request, ct);
            return classType is null ? Results.NotFound() : Results.Ok(classType);
        })
        .WithName("UpdateClassType")
        .WithSummary("Update a class type")
        .WithDescription("Updates an existing class type.")
        .Produces<ClassTypeResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
