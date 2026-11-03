namespace GroupPreOrder.Api.DTOs;

public class SubmitGroupOrderRequest
{
    public Guid ReservationId { get; set; }
}

public class ConsolidatedItemDto
{
    public Guid DishId { get; set; }

    public int Quantity { get; set; }
}