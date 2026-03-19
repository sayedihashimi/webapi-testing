using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class FineEndpoints
{
    public static RouteGroupBuilder MapFineEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/fines")
            .WithTags("Fines");

        group.MapGet("/", GetFinesAsync);
        group.MapGet("/{id:int}", GetFineByIdAsync);
        group.MapPost("/{id:int}/pay", PayFineAsync);
        group.MapPost("/{id:int}/waive", WaiveFineAsync);

        return group;
    }

    private static async Task<IResult> GetFinesAsync(
        IFineService service,
        string? status = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetFinesAsync(status, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetFineByIdAsync(
        int id,
        IFineService service,
        CancellationToken ct = default)
    {
        var fine = await service.GetFineByIdAsync(id, ct);
        return fine is not null
            ? TypedResults.Ok(fine)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> PayFineAsync(
        int id,
        IFineService service,
        CancellationToken ct = default)
    {
        var (fine, error) = await service.PayFineAsync(id, ct);

        if (error is not null)
        {
            return TypedResults.Problem(detail: error, statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Ok(fine);
    }

    private static async Task<IResult> WaiveFineAsync(
        int id,
        IFineService service,
        CancellationToken ct = default)
    {
        var (fine, error) = await service.WaiveFineAsync(id, ct);

        if (error is not null)
        {
            return TypedResults.Problem(detail: error, statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Ok(fine);
    }
}
