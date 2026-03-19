using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class LoanEndpoints
{
    public static RouteGroupBuilder MapLoanEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/loans")
            .WithTags("Loans");

        group.MapGet("/", GetLoansAsync);
        group.MapGet("/overdue", GetOverdueLoansAsync);
        group.MapGet("/{id:int}", GetLoanByIdAsync);
        group.MapPost("/", CheckoutBookAsync);
        group.MapPost("/{id:int}/return", ReturnBookAsync);
        group.MapPost("/{id:int}/renew", RenewLoanAsync);

        return group;
    }

    private static async Task<IResult> GetLoansAsync(
        ILoanService service,
        string? status = null,
        bool? overdue = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetLoansAsync(status, overdue, fromDate, toDate, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetOverdueLoansAsync(
        ILoanService service,
        CancellationToken ct = default)
    {
        var loans = await service.GetOverdueLoansAsync(ct);
        return TypedResults.Ok(loans);
    }

    private static async Task<IResult> GetLoanByIdAsync(
        int id,
        ILoanService service,
        CancellationToken ct = default)
    {
        var loan = await service.GetLoanByIdAsync(id, ct);
        return loan is not null
            ? TypedResults.Ok(loan)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CheckoutBookAsync(
        CreateLoanDto dto,
        ILoanService service,
        CancellationToken ct = default)
    {
        var (loan, error) = await service.CheckoutBookAsync(dto, ct);

        if (error is not null)
        {
            return TypedResults.Problem(detail: error, statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Created($"/api/loans/{loan!.Id}", loan);
    }

    private static async Task<IResult> ReturnBookAsync(
        int id,
        ILoanService service,
        CancellationToken ct = default)
    {
        var (loan, error) = await service.ReturnBookAsync(id, ct);

        if (error is not null)
        {
            return TypedResults.Problem(detail: error, statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Ok(loan);
    }

    private static async Task<IResult> RenewLoanAsync(
        int id,
        ILoanService service,
        CancellationToken ct = default)
    {
        var (loan, error) = await service.RenewLoanAsync(id, ct);

        if (error is not null)
        {
            return TypedResults.Problem(detail: error, statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Ok(loan);
    }
}
