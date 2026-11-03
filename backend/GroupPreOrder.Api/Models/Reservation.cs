namespace GroupPreOrder.Api.Models;

public class Reservation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OwnerId { get; set; }

    public GroupReservation? GroupReservation { get; set; }
}