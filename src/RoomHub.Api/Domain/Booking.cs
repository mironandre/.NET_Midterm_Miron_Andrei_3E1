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

        // R6: the maximum duration is 4 hours (exactly 4 hours is allowed).
        if (endUtc - startUtc > MaxDuration)
            return Error.Validation("DurationTooLong", "A booking cannot last more than 4 hours.");

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
    public Result<Booking> Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            return Error.Conflict("AlreadyCancelled", "The booking is already cancelled.");

        Status = BookingStatus.Cancelled;
        return this;
    }
}
