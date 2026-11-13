namespace CompeTournament.Mobile.Core.Services
{
    using System.Threading.Tasks;

    public sealed class NoOpDeviceTokenRegistrar : IDeviceTokenRegistrar
    {
        public Task RegisterAsync() => Task.CompletedTask;

        public Task UnregisterAsync() => Task.CompletedTask;
    }
}
