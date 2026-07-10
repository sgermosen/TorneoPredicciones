namespace CompeTournament.Mobile.Core.Services
{
    using CompeTournament.Shared.Live;
    using Microsoft.AspNetCore.SignalR.Client;

    public class LiveTournamentClient : ILiveTournamentClient
    {
        private readonly HubConnection _connection;

        public LiveTournamentClient(HubConnection connection)
        {
            _connection = connection;
            _connection.On<LiveMatchClosedDto>("MatchClosed", payload => MatchClosed?.Invoke(payload));
        }

        public event Action<LiveMatchClosedDto>? MatchClosed;

        public async Task JoinGroupAsync(int groupId)
        {
            await EnsureConnectedAsync();
            await _connection.InvokeAsync("JoinGroup", groupId);
        }

        public async Task LeaveGroupAsync(int groupId)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("LeaveGroup", groupId);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _connection.DisposeAsync();
        }

        private async Task EnsureConnectedAsync()
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
            }
        }
    }
}
