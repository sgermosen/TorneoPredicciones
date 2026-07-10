namespace CompeTournament.Mobile.Core.Services
{
    using CompeTournament.Shared.Auth;
    using CompeTournament.Shared.Tournaments;

    public interface IApiClient
    {
        Task<TokenResponse> LoginAsync(TokenRequest request);

        Task<UserDto> RegisterAsync(RegisterRequest request);

        Task<UserDto> GetMeAsync();

        Task<List<GroupDto>> GetGroupsAsync();

        Task<List<GroupDto>> GetMyGroupsAsync();

        Task<GroupDetailDto> GetGroupAsync(int id);

        Task JoinGroupAsync(int id);

        Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int id);

        Task<MatchDto> GetMatchAsync(int id);

        Task<PredictionDto> SavePredictionAsync(PredictionRequest request);

        Task<List<PredictionDto>> GetMyPredictionsAsync(int? groupId = null);
    }
}
