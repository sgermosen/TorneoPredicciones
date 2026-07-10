namespace CompeTournament.Backend.Realtime
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TournamentHub : Hub
    {
        public static string GroupName(int groupId) => $"group-{groupId}";

        public Task JoinGroup(int groupId) => Groups.AddToGroupAsync(Context.ConnectionId, GroupName(groupId));

        public Task LeaveGroup(int groupId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(groupId));
    }
}
