using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class LoanEndpoints
{
    public static RouteGroupBuilder MapLoanEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/loans").WithTags("Loans");

        group.MapGet("/", async (LoanStatus? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int? page, int? pageSize, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(status, overdue, fromDate, toDate, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetLoans")
        .WithSummary("List loans")
        .WithDescription("Returns a paginated list of loans. Filter by status, overdue flag, or date range.")
        .Produces<PaginatedResponse<LoanResponse>>();

        group.MapGet("/overdue", async (int? page, int? pageSize, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.GetOverdueAsync(page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetOverdueLoans")
        .WithSummary("Get overdue loans")
        .WithDescription("Returns all overdue loans. Also flags any active loans that are now past due.")
        .Produces<PaginatedResponse<LoanResponse>>();

        group.MapGet("/{id:int}", async (int id, ILoanService service, CancellationToken ct) =>
        {
            var loan = await service.GetByIdAsync(id, ct);
            return loan is null ? Results.NotFound() : Results.Ok(loan);
        })
        .WithName("GetLoanById")
        .WithSummary("Get loan by ID")
        .WithDescription("Returns the details of a specific loan.")
        .Produces<LoanResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateLoanRequest request, ILoanService service, CancellationToken ct) =>
        {
            var loan = await service.CheckoutAsync(request, ct);
            return Results.Created($"/api/loans/{loan.Id}", loan);
        })
        .WithName("CheckoutBook")
        .WithSummary("Checkout a book")
        .WithDescription("Creates a new loan (checkout). Enforces borrowing limits, fine thresholds, and availability.")
        .Produces<LoanResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/return", async (int id, ILoanService service, CancellationToken ct) =>
        {
            var loan = await service.ReturnAsync(id, ct);
            return Results.Ok(loan);
        })
        .WithName("ReturnBook")
        .WithSummary("Return a book")
        .WithDescription("Processes a book return. Generates overdue fines if applicable and checks reservation queue.")
        .Produces<LoanResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/renew", async (int id, ILoanService service, CancellationToken ct) =>
        {
            var loan = await service.RenewAsync(id, ct);
            return Results.Ok(loan);
        })
        .WithName("RenewLoan")
        .WithSummary("Renew a loan")
        .WithDescription("Renews a loan extending the due date. Max 2 renewals. Cannot renew if overdue, has pending reservations, or has too many fines.")
        .Produces<LoanResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        return group;
    }
}
