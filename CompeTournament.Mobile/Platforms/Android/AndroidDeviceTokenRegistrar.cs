using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using Microsoft.Maui.Storage;

namespace CompeTournament.Mobile
{
    public sealed class AndroidDeviceTokenRegistrar : IDeviceTokenRegistrar
    {
        private readonly IApiClient _api;

        public AndroidDeviceTokenRegistrar(IApiClient api)
        {
            _api = api;
        }

        public async Task RegisterAsync()
        {
            var token = Preferences.Get("fcm_token", null);
            if (!string.IsNullOrEmpty(token))
            {
                await _api.RegisterDeviceAsync(token, "android");
            }
        }

        public async Task UnregisterAsync()
        {
            var token = Preferences.Get("fcm_token", null);
            if (!string.IsNullOrEmpty(token))
            {
                await _api.UnregisterDeviceAsync(token);
            }
        }
    }
}
