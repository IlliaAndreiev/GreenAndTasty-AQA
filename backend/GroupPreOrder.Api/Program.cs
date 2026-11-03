using GroupPreOrder.Api.Data;
using GroupPreOrder.Api.Services;
using GroupPreOrder.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<GroupSessionsService>();

builder.Services.AddScoped<OwnerAuthorizationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment() ||
    app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();


if (app.Environment.IsEnvironment("Testing"))
{
    app.MapGet("/aqa/test-data", async (AppDbContext db) =>
        {
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

        db.GroupReservations.Add(group);
        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            OwnerId = ownerId,
            ReservationId = reservation.Id,
            GroupReservationId = group.Id
        });
    });

        app.MapPost("/aqa/empty-group", async (AppDbContext db) =>
    {
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

        db.GroupReservations.Add(group);
        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            OwnerId = ownerId,
            ReservationId = reservation.Id,
            GroupReservationId = group.Id
        });
    });

    app.MapDelete("/aqa/empty-group/{groupId:guid}",
    async (Guid groupId, AppDbContext db) =>
    {
        var group = await db.GroupReservations
            .Include(g => g.Reservation)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            return Results.NotFound();

        db.GroupReservations.Remove(group);
        db.Reservations.Remove(group.Reservation);

        await db.SaveChangesAsync();

        return Results.NoContent();
    });
}


app.Run();
