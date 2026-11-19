using RoomHub.Api.Domain;
using RoomHub.Api.Features;

namespace RoomHub.Api.Features.Bookings.CreateBooking;

public record CreateBookingRequest(string? BookedBy, DateTime StartUtc, DateTime EndUtc, int Attendees);

public record CreateBookingCommand(Guid RoomId, string? BookedBy, DateTime StartUtc, DateTime EndUtc, int Attendees);

public record BookingCreatedResponse(Guid Id, Guid RoomId, string Status);

public sealed class CreateBookingHandler(
    IRoomRepository rooms,
    IBookingRepository bookings,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateBookingCommand, BookingCreatedResponse>
{
    public async Task<Result<BookingCreatedResponse>> HandleAsync(CreateBookingCommand command, CancellationToken ct)
    {
        // Contract: when the room does not exist, 404 is returned before any validation.
        var room = await rooms.GetAsync(command.RoomId, ct);
        if (room is null)
            return Error.NotFound("RoomNotFound", "The room does not exist.");

        // R2–R6 enforced inside the domain.
        var result = Booking.Create(
            room,
            command.BookedBy,
            command.StartUtc,
            command.EndUtc,
            command.Attendees,
            timeProvider.GetUtcNow());

        if (result.IsFailure)
            return result.Error!;

        var booking = result.Value;

        // R7: two active bookings of the same room cannot overlap (touching is allowed).
        var active = await bookings.ListActiveForRoomAsync(command.RoomId, ct);
        if (active.Any(b => b.OverlapsWith(booking.StartUtc, booking.EndUtc)))
            return Error.Conflict("RoomOccupied", "The room is occupied in this interval.");

        // R9: a participant cannot have more than 2 active bookings per UTC day, across all rooms.
        var dayUtc = DateOnly.FromDateTime(booking.StartUtc);
        var sameDayCount = await bookings.CountActiveForUserOnDayAsync(booking.BookedBy, dayUtc, ct);
        if (sameDayCount >= 2)
            return Error.Conflict(
                "DailyBookingLimitReached",
                "A participant cannot have more than 2 active bookings on the same day.");

        await bookings.AddAsync(booking, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new BookingCreatedResponse(booking.Id, booking.RoomId, booking.Status.ToString());
    }
}

public static class CreateBookingEndpoint
{
    public static void MapCreateBooking(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/rooms/{roomId:guid}/bookings", async (
            Guid roomId,
            CreateBookingRequest request,
            CreateBookingHandler handler,
            CancellationToken ct) =>
        {
            var command = new CreateBookingCommand(
                roomId, request.BookedBy, request.StartUtc, request.EndUtc, request.Attendees);

            var result = await handler.HandleAsync(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/bookings/{result.Value.Id}", result.Value)
                : result.Error!.ToProblem();
        });
    }
}
