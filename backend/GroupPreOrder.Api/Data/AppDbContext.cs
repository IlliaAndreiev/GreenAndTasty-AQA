using GroupPreOrder.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupPreOrder.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<GroupReservation> GroupReservations => Set<GroupReservation>();
    public DbSet<GuestSession> GuestSessions => Set<GuestSession>();
    public DbSet<GuestSessionItem> GuestSessionItems => Set<GuestSessionItem>();
    public DbSet<Dish> Dishes => Set<Dish>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.GroupReservation)
            .WithOne(g => g.Reservation)
            .HasForeignKey<GroupReservation>(g => g.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupReservation>()
            .HasOne(g => g.Order)
            .WithOne(o => o.GroupReservation)
            .HasForeignKey<Order>(o => o.GroupReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GuestSession>()
            .HasMany(s => s.Items)
            .WithOne(i => i.GuestSession)
            .HasForeignKey(i => i.GuestSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GuestSessionItem>()
            .HasOne(i => i.Dish)
            .WithMany()
            .HasForeignKey(i => i.DishId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Dish)
            .WithMany()
            .HasForeignKey(i => i.DishId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}