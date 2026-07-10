using CompeTournament.Mobile.Core.Services;

namespace CompeTournament.Mobile.Services
{
    public class ShellNavigationService : INavigationService
    {
        public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
        {
            return parameters == null
                ? Shell.Current.GoToAsync(route)
                : Shell.Current.GoToAsync(route, parameters);
        }

        public Task GoBackAsync() => Shell.Current.GoToAsync("..");
    }
}
