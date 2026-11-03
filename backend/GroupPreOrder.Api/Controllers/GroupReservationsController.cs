using GroupPreOrder.Api.Data;
using GroupPreOrder.Api.DTOs;
using GroupPreOrder.Api.Models;
using GroupPreOrder.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupPreOrder.Api.Controllers;

[ApiController]
[Route("group-reservations")]
public class GroupReservationsController : ControllerBase
{
    private readonly AppDbContext _db;

    private readonly OwnerAuthorizationService _ownerAuth;
    public GroupReservationsController(
        AppDbContext db,
        OwnerAuthorizationService ownerAuth)
    {
        _db = db;
        _ownerAuth = ownerAuth;
    }

    [HttpPost("{groupReservationId:guid}/submit")]
    public async Task<ActionResult<List<ConsolidatedItemDto>>> Submit(
        Guid groupReservationId,
        [FromBody] SubmitGroupOrderRequest request)
    {

        var reservationExists = await _db.Reservations
            .AnyAsync(r => r.Id == request.ReservationId);

        if (!reservationExists)
            return NotFound("Reservation not found.");

        var group = await _db.GroupReservations
            .FirstOrDefaultAsync(g => g.Id == groupReservationId);

        if (group == null)
            return NotFound("Group reservation not found.");

        if (group.ReservationId != request.ReservationId)
            return BadRequest("Reservation does not belong to this group.");

        if (!Request.Headers.TryGetValue(
                "X-User-Id", out var userIdHeader) ||
            !Guid.TryParse(userIdHeader.ToString(), out var userId))
        {
            return Unauthorized("Missing or invalid X-User-Id.");
        }

        var isOwner = await _ownerAuth.IsOwnerAsync(
            request.ReservationId, userId);

        if (!isOwner)
            return StatusCode(403, "Only Owner can submit this order.");

        if (group.Status == GroupReservationStatus.Submitted)
            return BadRequest("Group reservation is already submitted.");

        var sessions = await _db.GuestSessions
            .AsNoTracking()
            .Where(s =>
                s.GroupReservationId == groupReservationId &&
                s.IsSubmitted)
            .Include(s => s.Items)
            .ToListAsync();

        if (sessions.Count == 0)
            return BadRequest("No submitted sessions found.");

        var consolidatedItems = sessions
            .SelectMany(s => s.Items)
            .GroupBy(i => i.DishId)
            .Select(g => new ConsolidatedItemDto
            {
                DishId = g.Key,
                Quantity = g.Sum(i => i.Quantity)
            })
            .ToList();

        if (consolidatedItems.Count == 0)
            return BadRequest("Submitted sessions contain no dishes.");

        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o =>
                o.GroupReservationId == groupReservationId);

        if (order == null)
            return BadRequest("Order not found.");

        await using var transaction =
            await _db.Database.BeginTransactionAsync();


        _db.OrderItems.RemoveRange(order.Items);

        foreach (var item in consolidatedItems)
        {
            var newOrderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                DishId = item.DishId,
                Quantity = item.Quantity
            };

            _db.OrderItems.Add(newOrderItem);
        }


        order.Status = OrderStatus.SubmittedByCustomer;
        group.Status = GroupReservationStatus.Submitted;



        try
        {
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(consolidatedItems);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            return StatusCode(
                500,
                "Failed to save the group order."
            );
        }


    }

}