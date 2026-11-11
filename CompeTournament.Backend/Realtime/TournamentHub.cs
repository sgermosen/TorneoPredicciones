namespace CompeTournament.Backend.Realtime
{
    using CompeTournament.Backend.Data;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;
    using System.Threading.Tasks;

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TournamentHub : Hub
    {
        public static string GroupName(int groupId) => $"group-{groupId}";

        private readonly ApplicationDbContext _context;

        public TournamentHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task JoinGroup(int groupId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var isMember = await _context.GroupUsers
                .AnyAsync(gu => gu.GroupId == groupId && gu.ApplicationUserId == userId && gu.IsAccepted && !gu.IsBlocked);
            if (!isMember)
            {
                throw new HubException("No eres miembro de este grupo.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(groupId));
        }

        public Task LeaveGroup(int groupId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(groupId));
    }
}
