namespace CompeTournament.Backend.Helpers
{
    using System.Threading.Tasks;

    public interface IMatchClosingService
    {
        Task<bool> CloseMatchAsync(int matchId, int localPoints, int visitorPoints);
    }
}
