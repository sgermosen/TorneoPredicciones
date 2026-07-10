namespace CompeTournament.Mobile.Core.Services
{
    public interface ITokenStore
    {
        Task<string?> GetTokenAsync();

        Task SetTokenAsync(string token);

        Task ClearAsync();
    }
}
