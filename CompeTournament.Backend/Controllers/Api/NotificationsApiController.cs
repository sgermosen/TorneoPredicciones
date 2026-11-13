namespace CompeTournament.Backend.Controllers.Api
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Shared.Notifications;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("api/notifications")]
    public class NotificationsApiController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> Mine()
        {
            var userId = CurrentUserId;

            var notifications = await _context.Notifications.AsNoTracking()
                .Where(n => n.ApplicationUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ThenByDescending(n => n.Id)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Body = n.Body,
                    IsRead = n.ReadAt != null,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> UnreadCount()
        {
            var userId = CurrentUserId;

            var count = await _context.Notifications.AsNoTracking()
                .CountAsync(n => n.ApplicationUserId == userId && n.ReadAt == null);

            return Ok(count);
        }

        [HttpPost("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = CurrentUserId;

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.ApplicationUserId == userId);
            if (notification == null)
            {
                return NotFound();
            }

            if (notification.ReadAt == null)
            {
                notification.ReadAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = CurrentUserId;

            var unread = await _context.Notifications
                .Where(n => n.ApplicationUserId == userId && n.ReadAt == null)
                .ToListAsync();

            if (unread.Count > 0)
            {
                var now = DateTime.Now;
                foreach (var notification in unread)
                {
                    notification.ReadAt = now;
                }

                await _context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
