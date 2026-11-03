using GroupPreOrder.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GroupPreOrder.Api.Services;

public class OwnerAuthorizationService
{
    private readonly AppDbContext _db;

    public OwnerAuthorizationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsOwnerAsync(
        Guid reservationId,
        Guid userId)
    {
        return await _db.Reservations.AnyAsync(r =>
            r.Id == reservationId &&
            r.OwnerId == userId);
    }
}