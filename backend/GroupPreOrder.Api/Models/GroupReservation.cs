namespace GroupPreOrder.Api.Models;

public class GroupReservation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;

    public GroupReservationStatus Status { get; set; }
        = GroupReservationStatus.Draft;

    public List<GuestSession> Sessions { get; set; } = new();

    public Order? Order { get; set; }
}

public enum GroupReservationStatus
{
    Draft,
    Submitted
}