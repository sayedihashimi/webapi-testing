using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class ClassTypeEndpoints
{
    public static RouteGroupBuilder MapClassTypeEndpoints(this RouteGroupBuilder group)
    {
        var types = group.MapGroup("/class-types").WithTags("Class Types");

        types.MapGet("/", GetAllAsync)
            .WithSummary("List class types with optional filters");

        types.MapGet("/{id:int}", GetByIdAsync)
            .WithSummary("Get class type details");

        types.MapPost("/", CreateAsync)
            .WithSummary("Create a new class type");

        types.MapPut("/{id:int}", UpdateAsync)
            .WithSummary("Update a class type");

        return group;
    }

    private static async Task<Ok<IReadOnlyList<ClassTypeResponse>>> GetAllAsync(
        IClassTypeService service,
        string? difficulty, bool? isPremium,
        CancellationToken ct)
    {
        var types = await service.GetAllAsync(difficulty, isPremium, ct);
        return TypedResults.Ok(types);
    }

    private static async Task<Results<Ok<ClassTypeResponse>, NotFound>> GetByIdAsync(
        int id, IClassTypeService service, CancellationToken ct)
    {
        var type = await service.GetByIdAsync(id, ct);
        return type is not null
            ? TypedResults.Ok(type)
            : TypedResults.NotFound();
    }

    private static async Task<Created<ClassTypeResponse>> CreateAsync(
        CreateClassTypeRequest request, IClassTypeService service, CancellationToken ct)
    {
        var type = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/class-types/{type.Id}", type);
    }

    private static async Task<Results<Ok<ClassTypeResponse>, NotFound>> UpdateAsync(
        int id, UpdateClassTypeRequest request, IClassTypeService service, CancellationToken ct)
    {
        var type = await service.UpdateAsync(id, request, ct);
        return type is not null
            ? TypedResults.Ok(type)
            : TypedResults.NotFound();
    }
}
