using System.Collections.Generic;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;

namespace CompeTournament.Tests
{
    public class FakeNavigationService : INavigationService
    {
        public List<string> Routes { get; } = new();

        public int BackCount { get; private set; }

        public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
        {
            Routes.Add(route);
            return Task.CompletedTask;
        }

        public Task GoBackAsync()
        {
            BackCount++;
            return Task.CompletedTask;
        }
    }
}
