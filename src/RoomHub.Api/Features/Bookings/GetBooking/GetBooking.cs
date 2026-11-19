using RoomHub.Api.Domain;

namespace RoomHub.Api.Features.Bookings.GetBooking;

public record GetBookingQuery(Guid Id);

public record BookingResponse(
    Guid Id,
    Guid RoomId,
    string BookedBy,
    DateTime StartUtc,
    DateTime EndUtc,
    int Attendees,
    string Status);

public sealed class GetBookingHandler(IBookingRepository bookings)
    : ICommandHandler<GetBookingQuery, BookingResponse>
{
    public async Task<Result<BookingResponse>> HandleAsync(GetBookingQuery query, CancellationToken ct)
    {
        var booking = await bookings.GetAsync(query.Id, ct);
        if (booking is null)
            return Error.NotFound("BookingNotFound", "The booking does not exist.");

        return new BookingResponse(
            booking.Id,
            booking.RoomId,
            booking.BookedBy,
            booking.StartUtc,
            booking.EndUtc,
            booking.Attendees,
            booking.Status.ToString());
    }
}

public static class GetBookingEndpoint
{
    public static void MapGetBooking(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/bookings/{id:guid}", async (
            Guid id,
            GetBookingHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetBookingQuery(id), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Error!.ToProblem();
        });
    }
}
