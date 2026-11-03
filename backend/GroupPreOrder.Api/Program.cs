using GroupPreOrder.Api.Data;
using GroupPreOrder.Api.Services;
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
        var data = await db.GroupReservations
            .AsNoTracking()
            .Select(g => new
            {
                OwnerId = g.Reservation.OwnerId,
                ReservationId = g.ReservationId,
                GroupReservationId = g.Id
            })
            .FirstOrDefaultAsync();

        if (data == null)
        {
            return Results.NotFound(
                "No seeded group reservation found."
            );
        }

        return Results.Ok(data);
    });
}


app.Run();
