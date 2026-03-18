using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class BookingEndpoints
{
    public static RouteGroupBuilder MapBookingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/bookings").WithTags("Bookings");

        group.MapPost("/", async Task<Results<Created<BookingResponse>, NotFound<string>, Conflict<string>>> (CreateBookingRequest request, BookingService service, CancellationToken ct) =>
        {
            var (error, result) = await service.CreateAsync(request, ct);
            if (error is null)
            {
                return TypedResults.Created($"/api/bookings/{result!.Id}", result);
            }

            return error is "member_not_found" or "class_not_found"
                ? TypedResults.NotFound(error switch
                {
                    "member_not_found" => "Member not found.",
                    _ => "Class not found."
                })
                : TypedResults.Conflict(error switch
                {
                    "member_inactive" => "Member account is inactive.",
                    "class_not_available" => "Class is not available for booking.",
                    "too_far_in_advance" => "Cannot book more than 7 days in advance.",
                    "too_late_to_book" => "Cannot book less than 30 minutes before class start.",
                    "no_active_membership" => "Active membership required to book classes.",
                    "premium_access_denied" => "Your membership plan does not include premium classes.",
                    "weekly_limit_reached" => "Weekly booking limit reached for your membership plan.",
                    "double_booking" => "You have an overlapping class booking at this time.",
                    "already_booked" => "You have already booked this class.",
                    _ => "Booking failed."
                });
        });

        group.MapGet("/{id:int}", async Task<Results<Ok<BookingResponse>, NotFound<string>>> (int id, BookingService service, CancellationToken ct) =>
        {
            var booking = await service.GetByIdAsync(id, ct);
            return booking is not null
                ? TypedResults.Ok(booking)
                : TypedResults.NotFound("Booking not found.");
        });

        group.MapPost("/{id:int}/cancel", async Task<Results<Ok<string>, NotFound<string>, Conflict<string>>> (int id, CancelBookingRequest? request, BookingService service, CancellationToken ct) =>
        {
            var result = await service.CancelAsync(id, request?.Reason, ct);
            return result switch
            {
                null => TypedResults.Ok("Booking cancelled."),
                "not_found" => TypedResults.NotFound("Booking not found."),
                "cannot_cancel" => TypedResults.Conflict("Booking cannot be cancelled in its current status."),
                _ => TypedResults.Conflict("Cannot cancel a booking for a class that has already started or completed.")
            };
        });

        group.MapPost("/{id:int}/check-in", async Task<Results<Ok<string>, NotFound<string>, Conflict<string>>> (int id, BookingService service, CancellationToken ct) =>
        {
            var result = await service.CheckInAsync(id, ct);
            return result switch
            {
                null => TypedResults.Ok("Checked in successfully."),
                "not_found" => TypedResults.NotFound("Booking not found."),
                "not_confirmed" => TypedResults.Conflict("Only confirmed bookings can be checked in."),
                _ => TypedResults.Conflict("Check-in is only available 15 minutes before to 15 minutes after class start.")
            };
        });

        group.MapPost("/{id:int}/no-show", async Task<Results<Ok<string>, NotFound<string>, Conflict<string>>> (int id, BookingService service, CancellationToken ct) =>
        {
            var result = await service.MarkNoShowAsync(id, ct);
            return result switch
            {
                null => TypedResults.Ok("Marked as no-show."),
                "not_found" => TypedResults.NotFound("Booking not found."),
                "not_confirmed" => TypedResults.Conflict("Only confirmed bookings can be marked as no-show."),
                _ => TypedResults.Conflict("Can only mark no-show after 15 minutes past class start.")
            };
        });

        return group;
    }
}
