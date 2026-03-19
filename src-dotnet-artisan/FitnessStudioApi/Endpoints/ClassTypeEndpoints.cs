using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class ClassTypeEndpoints
{
    public static RouteGroupBuilder MapClassTypeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/class-types")
            .WithTags("Class Types");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);

        return group;
    }

    private static async Task<IResult> GetAllAsync(
        DifficultyLevel? difficulty, bool? isPremium,
        IClassTypeService service, CancellationToken ct)
    {
        var types = await service.GetAllAsync(difficulty, isPremium, ct);
        return TypedResults.Ok(types);
    }

    private static async Task<IResult> GetByIdAsync(
        int id, IClassTypeService service, CancellationToken ct)
    {
        var classType = await service.GetByIdAsync(id, ct);
        return classType is not null
            ? TypedResults.Ok(classType)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateAsync(
        CreateClassTypeRequest request, IClassTypeService service, CancellationToken ct)
    {
        try
        {
            var classType = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/class-types/{classType.Id}", classType);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateAsync(
        int id, UpdateClassTypeRequest request, IClassTypeService service, CancellationToken ct)
    {
        try
        {
            var classType = await service.UpdateAsync(id, request, ct);
            return classType is not null
                ? TypedResults.Ok(classType)
                : TypedResults.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(new { error = ex.Message });
        }
    }
}
