namespace CompeTournament.Mobile.Core.Services
{
    public interface ITokenStore
    {
        Task<string?> GetTokenAsync();

        Task<string?> GetRefreshTokenAsync();

        Task SetTokensAsync(string accessToken, string refreshToken);

        Task ClearAsync();
    }
}
