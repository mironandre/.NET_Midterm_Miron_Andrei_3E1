using RoomHub.Api.Domain;

namespace RoomHub.Api.Features.Bookings.CancelBooking;

public record CancelBookingCommand(Guid Id);

public sealed class CancelBookingHandler(IBookingRepository bookings, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : ICommandHandler<CancelBookingCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(CancelBookingCommand command, CancellationToken ct)
    {
        // R8: cancelling a missing booking gives 404.
        var booking = await bookings.GetAsync(command.Id, ct);
        if (booking is null)
            return Error.NotFound("BookingNotFound", "The booking does not exist.");

        // R8: already cancelled -> 409. R11: too close to start -> 409.
        var result = booking.Cancel(timeProvider.GetUtcNow());
        if (result.IsFailure)
            return result.Error!;

        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }
}

public static class CancelBookingEndpoint
{
    public static void MapCancelBooking(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/bookings/{id:guid}/cancel", async (
            Guid id,
            CancelBookingHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new CancelBookingCommand(id), ct);

            return result.IsSuccess
                ? Results.NoContent()
                : result.Error!.ToProblem();
        });
    }
}
