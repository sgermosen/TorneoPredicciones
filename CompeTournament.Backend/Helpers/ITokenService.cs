namespace CompeTournament.Backend.Helpers
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Shared.Auth;
    using System.Threading.Tasks;

    public interface ITokenService
    {
        Task<TokenResponse> CreateTokenAsync(ApplicationUser user, int points);
    }
}
