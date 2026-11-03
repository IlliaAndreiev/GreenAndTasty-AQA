using GroupPreOrder.Api.Data;
using GroupPreOrder.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GroupPreOrder.Api.Services;

public class GroupSessionsService
{
    private readonly AppDbContext _db;

    public GroupSessionsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GroupSessionsResponse> GetSnapshotAsync(
        Guid groupReservationId)
    {
        var sessions = await _db.GuestSessions
            .AsNoTracking()
            .Where(s => s.GroupReservationId == groupReservationId)
            .Include(s => s.Items)
                .ThenInclude(i => i.Dish)
            .ToListAsync();

        var participants = sessions.Select(s => new ParticipantDto
        {
            Id = s.Id,
            Type = s.Type.ToString(),
            Name = s.Name,
            IsSubmitted = s.IsSubmitted,

            Items = s.Items.Select(i => new SessionItemDto
            {
                Dish = new DishDto
                {
                    Id = i.Dish.Id,
                    Name = i.Dish.Name,
                    Price = i.Dish.Price
                },
                Quantity = i.Quantity
            }).ToList(),

            Total = s.Items.Sum(i => i.Dish.Price * i.Quantity)
        }).ToList();

        return new GroupSessionsResponse
        {
            Participants = participants,
            Total = participants.Sum(p => p.Total)
        };
    }
}