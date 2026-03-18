using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class ClassTypeEndpoints
{
    public static void MapClassTypeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/class-types").WithTags("Class Types");

        group.MapGet("/", async (string? difficulty, bool? isPremium, int page = 1, int pageSize = 20, IClassTypeService service = default!, CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);
            var result = await service.GetAllAsync(difficulty, isPremium, page, pageSize, ct);
            return Results.Ok(result);
        })
        .WithName("GetClassTypes")
        .WithSummary("List class types")
        .WithDescription("Returns paginated list of class types. Filter by difficulty level and premium status.")
        .Produces<PagedResponse<ClassTypeResponse>>(StatusCodes.Status200OK);

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
            return TypedResults.Created($"/api/class-types/{classType.Id}", classType);
        })
        .WithName("CreateClassType")
        .WithSummary("Create a new class type")
        .WithDescription("Creates a new class type. Name must be unique.")
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
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
