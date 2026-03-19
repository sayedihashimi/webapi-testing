using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class ClassTypeEndpoints
{
    public static void MapClassTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/class-types").WithTags("Class Types");

        group.MapGet("/", async Task<Ok<IReadOnlyList<ClassTypeResponse>>> (
            string? difficulty, bool? isPremium,
            IClassTypeService service, CancellationToken ct) =>
        {
            var classTypes = await service.GetAllAsync(difficulty, isPremium, ct);
            return TypedResults.Ok(classTypes);
        })
        .WithName("GetClassTypes")
        .WithSummary("List class types")
        .WithDescription("Returns all active class types. Filter by difficulty level or premium status.")
        .Produces<IReadOnlyList<ClassTypeResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<ClassTypeResponse>, NotFound>> (
            int id, IClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.GetByIdAsync(id, ct);
            return classType is null ? TypedResults.NotFound() : TypedResults.Ok(classType);
        })
        .WithName("GetClassTypeById")
        .WithSummary("Get a class type by ID")
        .WithDescription("Returns the full details of a specific class type.")
        .Produces<ClassTypeResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<ClassTypeResponse>> (
            CreateClassTypeRequest request, IClassTypeService service, CancellationToken ct) =>
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

        group.MapPut("/{id:int}", async Task<Results<Ok<ClassTypeResponse>, NotFound>> (
            int id, UpdateClassTypeRequest request, IClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(classType);
        })
        .WithName("UpdateClassType")
        .WithSummary("Update a class type")
        .WithDescription("Updates an existing class type.")
        .Produces<ClassTypeResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
