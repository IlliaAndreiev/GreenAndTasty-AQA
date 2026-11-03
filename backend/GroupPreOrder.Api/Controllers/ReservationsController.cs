using GroupPreOrder.Api.Data;
using GroupPreOrder.Api.DTOs;
using GroupPreOrder.Api.Models;
using GroupPreOrder.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupPreOrder.Api.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationsController : ControllerBase
{
    private readonly AppDbContext _db;

    private readonly OwnerAuthorizationService _ownerAuth;
    public ReservationsController(
        AppDbContext db,
        OwnerAuthorizationService ownerAuth)
    {
        _db = db;
        _ownerAuth = ownerAuth;
    }

    [HttpGet("{reservationId:guid}/guest-session-id")]
    public async Task<ActionResult<OwnerSessionResponse>>
        GetOwnerSessionId(Guid reservationId)
    {
        var reservationExists = await _db.Reservations
            .AnyAsync(r => r.Id == reservationId);

        if (!reservationExists)
            return NotFound("Reservation not found.");


        if (!Request.Headers.TryGetValue(
                "X-User-Id", out var userIdHeader) ||
            !Guid.TryParse(userIdHeader.ToString(), out var userId))
        {
            return Unauthorized("Missing or invalid X-User-Id.");
        }

        var isOwner = await _ownerAuth.IsOwnerAsync(
            reservationId, userId);

        if (!isOwner)
            return StatusCode(403, "Only Owner can access this resource.");


        var ownerSession = await _db.GuestSessions
            .AsNoTracking()
            .Where(s =>
                s.GroupReservation.ReservationId == reservationId &&
                s.Type == GuestSessionType.Owner)
            .FirstOrDefaultAsync();

        if (ownerSession == null)
            return NotFound("Owner session not found.");

        return Ok(new OwnerSessionResponse
        {
            GuestSessionId = ownerSession.Id
        });
    }
}