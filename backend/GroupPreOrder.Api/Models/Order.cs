namespace GroupPreOrder.Api.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GroupReservationId { get; set; }
    public GroupReservation GroupReservation { get; set; } = null!;

    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    public List<OrderItem> Items { get; set; } = new();
}

public enum OrderStatus
{
    Draft,
    SubmittedByCustomer
}