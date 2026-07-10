namespace CompeTournament.Backend.Controllers.Api
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Data.Entities;
    using CompeTournament.Shared.Auth;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("api/devices")]
    public class DevicesApiController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DevicesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("mine")]
        public async Task<ActionResult<List<string>>> Mine()
        {
            var userId = CurrentUserId;
            var tokens = await _context.DeviceTokens
                .Where(d => d.ApplicationUserId == userId)
                .Select(d => d.Token)
                .AsNoTracking()
                .ToListAsync();

            return Ok(tokens);
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] DeviceRegistrationRequest request)
        {
            var userId = CurrentUserId;

            var existing = await _context.DeviceTokens
                .FirstOrDefaultAsync(d => d.Token == request.Token);

            if (existing == null)
            {
                _context.DeviceTokens.Add(new DeviceToken
                {
                    ApplicationUserId = userId,
                    Token = request.Token,
                    Platform = request.Platform,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.ApplicationUserId = userId;
                existing.Platform = request.Platform;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Unregister([FromBody] DeviceRegistrationRequest request)
        {
            var tokens = await _context.DeviceTokens
                .Where(d => d.Token == request.Token)
                .ToListAsync();

            if (tokens.Count > 0)
            {
                _context.DeviceTokens.RemoveRange(tokens);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
