namespace CompeTournament.Mobile.Core.Services
{
    using System.Threading.Tasks;

    public interface IDeviceTokenRegistrar
    {
        Task RegisterAsync();

        Task UnregisterAsync();
    }
}
