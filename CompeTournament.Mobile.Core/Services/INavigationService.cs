namespace CompeTournament.Mobile.Core.Services
{
    public interface INavigationService
    {
        Task GoToAsync(string route, IDictionary<string, object>? parameters = null);

        Task GoBackAsync();
    }
}
