using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class LoanEndpoints
{
    public static void MapLoanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/loans").WithTags("Loans");

        group.MapGet("/", async Task<Ok<PaginatedResponse<LoanResponse>>> (
            LoanStatus? status, int? page, int? pageSize,
            ILoanService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(status, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetLoans")
        .WithSummary("List loans")
        .WithDescription("Returns a paginated list of loans, optionally filtered by status.")
        .Produces<PaginatedResponse<LoanResponse>>(StatusCodes.Status200OK);

        group.MapGet("/overdue", async Task<Ok<IReadOnlyList<LoanResponse>>> (
            ILoanService service, CancellationToken ct) =>
        {
            var result = await service.GetOverdueAsync(ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetOverdueLoans")
        .WithSummary("Get all overdue loans")
        .WithDescription("Returns all currently overdue loans and updates their status.")
        .Produces<IReadOnlyList<LoanResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<LoanDetailResponse>, NotFound>> (
            int id, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetLoanById")
        .WithSummary("Get loan by ID")
        .WithDescription("Returns loan details including associated fines.")
        .Produces<LoanDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<LoanResponse>> (
            CreateLoanRequest request, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.CheckoutAsync(request, ct);
            return TypedResults.Created($"/api/loans/{result.Id}", result);
        })
        .WithName("CheckoutBook")
        .WithSummary("Checkout a book")
        .WithDescription("Creates a new loan (checkout). Enforces borrowing limits, fine thresholds, and availability.")
        .Produces<LoanResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/return", async Task<Ok<LoanResponse>> (
            int id, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.ReturnAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("ReturnBook")
        .WithSummary("Return a book")
        .WithDescription("Processes a book return. Auto-generates fines for overdue returns and promotes pending reservations.")
        .Produces<LoanResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/renew", async Task<Ok<LoanResponse>> (
            int id, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.RenewAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("RenewLoan")
        .WithSummary("Renew a loan")
        .WithDescription("Renews a loan (max 2 renewals). Cannot renew if overdue, pending reservations exist, or fine threshold exceeded.")
        .Produces<LoanResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
