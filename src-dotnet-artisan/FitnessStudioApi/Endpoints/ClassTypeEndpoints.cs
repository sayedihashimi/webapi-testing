using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class ClassTypeEndpoints
{
    public static RouteGroupBuilder MapClassTypeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/class-types").WithTags("Class Types");

        group.MapGet("/", async Task<Ok<List<ClassTypeResponse>>> (string? difficulty, bool? isPremium, ClassTypeService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(difficulty, isPremium, ct)));

        group.MapGet("/{id:int}", async Task<Results<Ok<ClassTypeResponse>, NotFound<string>>> (int id, ClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.GetByIdAsync(id, ct);
            return classType is not null
                ? TypedResults.Ok(classType)
                : TypedResults.NotFound("Class type not found.");
        });

        group.MapPost("/", async Task<Results<Created<ClassTypeResponse>, Conflict<string>>> (CreateClassTypeRequest request, ClassTypeService service, CancellationToken ct) =>
        {
            try
            {
                var classType = await service.CreateAsync(request, ct);
                return TypedResults.Created($"/api/class-types/{classType.Id}", classType);
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        });

        group.MapPut("/{id:int}", async Task<Results<Ok<ClassTypeResponse>, NotFound<string>, Conflict<string>>> (int id, UpdateClassTypeRequest request, ClassTypeService service, CancellationToken ct) =>
        {
            try
            {
                var classType = await service.UpdateAsync(id, request, ct);
                return classType is not null
                    ? TypedResults.Ok(classType)
                    : TypedResults.NotFound("Class type not found.");
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        });

        return group;
    }
}
