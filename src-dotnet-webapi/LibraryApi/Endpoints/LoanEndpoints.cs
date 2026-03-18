using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class LoanEndpoints
{
    public static RouteGroupBuilder MapLoanEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/loans").WithTags("Loans");

        group.MapGet("/", async (LoanStatus? status, bool? overdue, int? page, int? pageSize, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(status, overdue, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetLoans")
        .WithSummary("List loans")
        .WithDescription("Returns a paginated list of loans. Supports filtering by status and overdue flag.")
        .Produces<PagedResponse<LoanResponse>>();

        group.MapGet("/overdue", async (ILoanService service, CancellationToken ct) =>
        {
            var result = await service.GetOverdueAsync(ct);
            return Results.Ok(result);
        })
        .WithName("GetOverdueLoans")
        .WithSummary("Get all overdue loans")
        .WithDescription("Returns all currently overdue loans.")
        .Produces<List<LoanResponse>>();

        group.MapGet("/{id:int}", async (int id, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetLoanById")
        .WithSummary("Get loan by ID")
        .WithDescription("Returns loan details including book and patron information.")
        .Produces<LoanResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateLoanRequest request, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.CheckoutAsync(request, ct);
            return TypedResults.Created($"/api/loans/{result.Id}", result);
        })
        .WithName("CheckoutBook")
        .WithSummary("Checkout a book")
        .WithDescription("Creates a new loan (checkout). Enforces borrowing limits, fine thresholds, availability, and active membership.")
        .Produces<LoanResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/return", async (int id, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.ReturnAsync(id, ct);
            return Results.Ok(result);
        })
        .WithName("ReturnBook")
        .WithSummary("Return a book")
        .WithDescription("Processes a book return. Auto-generates overdue fines and promotes pending reservations.")
        .Produces<LoanResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/renew", async (int id, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.RenewAsync(id, ct);
            return Results.Ok(result);
        })
        .WithName("RenewLoan")
        .WithSummary("Renew a loan")
        .WithDescription("Renews an active loan. Max 2 renewals. Cannot renew if overdue, has pending reservations, or unpaid fines >= $10.")
        .Produces<LoanResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
