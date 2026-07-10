namespace CompeTournament.Mobile.Core.Services
{
    using CompeTournament.Shared.Live;

    public interface ILiveTournamentClient : IAsyncDisposable
    {
        event Action<LiveMatchClosedDto>? MatchClosed;

        Task JoinGroupAsync(int groupId);

        Task LeaveGroupAsync(int groupId);
    }
}
