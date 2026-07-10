namespace CompeTournament.Mobile.Core.Services
{
    using CompeTournament.Shared.Auth;
    using CompeTournament.Shared.Tournaments;

    public interface IApiClient
    {
        Task<TokenResponse> LoginAsync(TokenRequest request);

        Task<TokenResponse> RefreshAsync(string refreshToken);

        Task LogoutAsync(string refreshToken);

        Task<UserDto> RegisterAsync(RegisterRequest request);

        Task<UserDto> GetMeAsync();

        Task<List<GroupDto>> GetGroupsAsync();

        Task<List<GroupDto>> GetMyGroupsAsync();

        Task<GroupDetailDto> GetGroupAsync(int id);

        Task JoinGroupAsync(int id);

        Task<GroupInviteDto> GetInviteAsync(int id);

        Task<GroupDto> JoinByCodeAsync(string code);

        Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int id);

        Task<RecapDto> GetRecapAsync(int id);

        Task<MatchDto> GetMatchAsync(int id);

        Task<PredictionDto> SavePredictionAsync(PredictionRequest request);

        Task<List<PredictionDto>> GetMyPredictionsAsync(int? groupId = null);

        Task<InsightsDto> GetInsightsAsync(int? groupId = null);

        Task<List<CommentDto>> GetCommentsAsync(int matchId);

        Task<CommentDto> PostCommentAsync(int matchId, string body);
    }
}
