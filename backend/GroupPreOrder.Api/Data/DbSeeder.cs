using GroupPreOrder.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupPreOrder.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {

        if (await db.Reservations.AnyAsync())
        {
            var existingGroup = await db.GroupReservations
                .AsNoTracking()
                .Select(g => new
                {
                    g.Id,
                    g.ReservationId,
                    g.Reservation.OwnerId
                })
                .FirstOrDefaultAsync();

            if (existingGroup != null)
            {
                Console.WriteLine("=== EXISTING TEST DATA ===");
                Console.WriteLine($"Owner ID: {existingGroup.OwnerId}");
                Console.WriteLine($"Reservation ID: {existingGroup.ReservationId}");
                Console.WriteLine($"Group Reservation ID: {existingGroup.Id}");
                Console.WriteLine("==========================");
            }

            return;
        }


        var ownerId = Guid.NewGuid();

        var reservation = new Reservation
        {
            OwnerId = ownerId
        };

        var group = new GroupReservation
        {
            Reservation = reservation,
            Status = GroupReservationStatus.Draft
        };

        var caesar = new Dish
        {
            Name = "Caesar Salad",
            Price = 180m
        };

        var tomYum = new Dish
        {
            Name = "Tom Yum",
            Price = 220m
        };

        var andrii = new GuestSession
        {
            GroupReservation = group,
            Name = "Andrii",
            Type = GuestSessionType.Guest,
            IsSubmitted = true,
            Items = new List<GuestSessionItem>
            {
                new()
                {
                    Dish = caesar,
                    Quantity = 2
                }
            }
        };

        var olena = new GuestSession
        {
            GroupReservation = group,
            Name = "Olena",
            Type = GuestSessionType.Guest,
            IsSubmitted = false,
            Items = new List<GuestSessionItem>
            {
                new()
                {
                    Dish = tomYum,
                    Quantity = 1
                }
            }
        };

        var ownerSession = new GuestSession
        {
            GroupReservation = group,
            Name = "Owner",
            Type = GuestSessionType.Owner,
            IsSubmitted = false
        };

        var order = new Order
        {
            GroupReservation = group,
            Status = OrderStatus.Draft
        };

        db.GuestSessions.AddRange(
            andrii,
            olena,
            ownerSession
        );

        db.Orders.Add(order);

        await db.SaveChangesAsync();

        Console.WriteLine("=== TEST DATA ===");
        Console.WriteLine($"Owner ID: {ownerId}");
        Console.WriteLine($"Reservation ID: {reservation.Id}");
        Console.WriteLine($"Group Reservation ID: {group.Id}");
        Console.WriteLine("=================");
    }
}