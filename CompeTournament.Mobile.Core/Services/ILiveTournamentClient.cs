namespace CompeTournament.Mobile.Core.Services
{
    using CompeTournament.Shared.Live;
    using CompeTournament.Shared.Tournaments;

    public interface ILiveTournamentClient : IAsyncDisposable
    {
        event Action<LiveMatchClosedDto>? MatchClosed;

        event Action<CommentDto>? CommentPosted;

        Task JoinGroupAsync(int groupId);

        Task LeaveGroupAsync(int groupId);
    }
}
