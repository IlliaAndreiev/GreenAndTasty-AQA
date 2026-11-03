using GroupPreOrder.Api.Data;
using GroupPreOrder.Api.DTOs;
using GroupPreOrder.Api.Models;
using GroupPreOrder.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupPreOrder.Api.Controllers;

[ApiController]
[Route("guest/groups")]
public class GroupSessionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly GroupSessionsService _sessionsService;
    private readonly OwnerAuthorizationService _ownerAuth;
  
    public GroupSessionsController(
        AppDbContext db,
        GroupSessionsService sessionsService,
        OwnerAuthorizationService ownerAuth)
    {
        _db = db;
        _sessionsService = sessionsService;
        _ownerAuth = ownerAuth;
    }


    [HttpGet("{groupReservationId:guid}/sessions")]
    public async Task<ActionResult<GroupSessionsResponse>> GetSessions(
        Guid groupReservationId)
    {
        // 1. Шукаємо групу
        var group = await _db.GroupReservations
            .FirstOrDefaultAsync(g => g.Id == groupReservationId);

        if (group == null)
            return NotFound("Group reservation not found.");

        // 2. Отримуємо ID користувача із заголовка
        if (!Request.Headers.TryGetValue(
                "X-User-Id", out var userIdHeader) ||
            !Guid.TryParse(userIdHeader.ToString(), out var userId))
        {
            return Unauthorized("Missing or invalid X-User-Id.");
        }

        // 3. Перевіряємо, чи користувач є Owner
        var isOwner = await _ownerAuth.IsOwnerAsync(
            group.ReservationId, userId);

        if (!isOwner)
            return StatusCode(403, "Only Owner can view group sessions.");

        // 4. Повертаємо дані лише після перевірки
        var response = await _sessionsService
            .GetSnapshotAsync(groupReservationId);

        return Ok(response);
    }



    [HttpDelete("{groupReservationId:guid}/sessions/{guestSessionId:guid}")]
    public async Task<ActionResult<GroupSessionsResponse>> DeleteSession(
        Guid groupReservationId,
        Guid guestSessionId)
    {
        // 1. Знаходимо групу
        var group = await _db.GroupReservations
            .FirstOrDefaultAsync(g => g.Id == groupReservationId);

        if (group == null)
            return NotFound("Group reservation not found.");

        // 2. Перевіряємо заголовок користувача
        if (!Request.Headers.TryGetValue(
                "X-User-Id", out var userIdHeader) ||
            !Guid.TryParse(userIdHeader.ToString(), out var userId))
        {
            return Unauthorized("Missing or invalid X-User-Id.");
        }

        // 3. Перевіряємо права Owner
        var isOwner = await _ownerAuth.IsOwnerAsync(
            group.ReservationId, userId);

        if (!isOwner)
            return StatusCode(403, "Only Owner can delete sessions.");

        // 4. Знаходимо сесію
        var session = await _db.GuestSessions
            .FirstOrDefaultAsync(s => s.Id == guestSessionId);

        if (session == null)
            return NotFound("Guest session not found.");

        // 5. Перевіряємо, чи належить сесія цій групі
        if (session.GroupReservationId != groupReservationId)
            return BadRequest("Session does not belong to this group.");

        if (session.Type != GuestSessionType.Guest)
        {
            return BadRequest("Only guest sessions can be deleted.");
        }

        // 6. Видаляємо сесію
        _db.GuestSessions.Remove(session);

        await _db.SaveChangesAsync();

        // 7. Повертаємо оновлений стан групи
        var response = await _sessionsService
            .GetSnapshotAsync(groupReservationId);

        return Ok(response);
    }

}