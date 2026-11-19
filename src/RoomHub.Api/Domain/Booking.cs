namespace RoomHub.Api.Domain;

public enum BookingStatus
{
    Active,
    Cancelled
}

public class Booking
{
    // The longest a booking may last (R6). Exactly 4 hours is allowed.
    public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(4);

    // R10: rooms with capacity greater than 10 may be booked for at most 2 hours.
    public static readonly TimeSpan LargeRoomMaxDuration = TimeSpan.FromHours(2);
    public const int LargeRoomCapacityThreshold = 10;

    // R11: a booking can be cancelled only if it starts at least this far from now.
    public static readonly TimeSpan MinNoticeToCancel = TimeSpan.FromMinutes(60);

    // Parameterless ctor for EF Core materialization.
    private Booking() { }

    private Booking(Guid id, Guid roomId, string bookedBy, DateTime startUtc, DateTime endUtc, int attendees)
    {
        Id = id;
        RoomId = roomId;
        BookedBy = bookedBy;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Attendees = attendees;
        Status = BookingStatus.Active;
    }

    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public string BookedBy { get; private set; } = string.Empty;
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public int Attendees { get; private set; }
    public BookingStatus Status { get; private set; }

    public bool IsActive => Status == BookingStatus.Active;

    // R2, R3, R4, R5, R6 live here.
    public static Result<Booking> Create(
        Room room,
        string? bookedBy,
        DateTime startUtc,
        DateTime endUtc,
        int attendees,
        DateTimeOffset now)
    {
        // R2: bookedBy is required.
        if (string.IsNullOrWhiteSpace(bookedBy))
            return Error.Validation("InvalidBookedBy", "BookedBy is required.");

        // R3: end must be strictly after start.
        if (endUtc <= startUtc)
            return Error.Validation("InvalidRange", "The end must be strictly after the start.");

        // R4: attendees strictly positive and at most the room capacity.
        if (attendees <= 0)
            return Error.Validation("InvalidAttendees", "Attendees must be strictly positive.");

        if (attendees > room.Capacity)
            return Error.Validation("AttendeesExceedCapacity", "Attendees exceed the room capacity.");

        // R5: a booking cannot start in the past ("now" comes from TimeProvider).
        if (startUtc < now.UtcDateTime)
            return Error.Validation("StartInPast", "A booking cannot start in the past.");

        // R6 / R10: the maximum duration depends on the room capacity.
        // Rooms with capacity > 10 are limited to 2 hours; others keep the 4-hour limit.
        var maxDuration = room.Capacity > LargeRoomCapacityThreshold
            ? LargeRoomMaxDuration
            : MaxDuration;

        if (endUtc - startUtc > maxDuration)
            return Error.Validation(
                "DurationTooLong",
                $"A booking in this room cannot last more than {maxDuration.TotalHours} hours.");

        return new Booking(Guid.NewGuid(), room.Id, bookedBy.Trim(), startUtc, endUtc, attendees);
    }

    // R7: two active bookings of the same room cannot overlap.
    // Intervals that only touch (one ends exactly when the other starts) do not overlap.
    public bool OverlapsWith(DateTime otherStartUtc, DateTime otherEndUtc)
    {
        if (!IsActive)
            return false;

        return StartUtc < otherEndUtc && otherStartUtc < EndUtc;
    }

    // R8: cancelling moves Active -> Cancelled; cancelling an already cancelled booking is a conflict.
    // R11: a booking can be cancelled only if it starts at least 60 minutes from now.
    public Result<Booking> Cancel(DateTimeOffset now)
    {
        if (Status == BookingStatus.Cancelled)
            return Error.Conflict("AlreadyCancelled", "The booking is already cancelled.");

        if (StartUtc - now.UtcDateTime < MinNoticeToCancel)
            return Error.Conflict(
                "CancelTooLate",
                "A booking can only be cancelled at least 60 minutes before it starts.");

        Status = BookingStatus.Cancelled;
        return this;
    }
}
