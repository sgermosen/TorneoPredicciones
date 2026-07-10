namespace CompeTournament.Backend.Helpers
{
    using System.Threading.Tasks;

    public interface IMatchScheduler
    {
        Task<int> LockDueMatchesAsync();

        Task<int> AutoCloseWithResultsAsync();
    }
}
