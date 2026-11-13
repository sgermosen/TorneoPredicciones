namespace CompeTournament.Mobile.Core.Services
{
    using CompeTournament.Shared.Auth;
    using CompeTournament.Shared.Notifications;
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

        Task<GroupDto> CreateGroupAsync(CreateGroupRequest request);

        Task<List<TournamentTypeDto>> GetTournamentTypesAsync();

        Task<List<LeaderboardEntryDto>> GetGroupMembersAsync(int id);

        Task AcceptMemberAsync(int id, string userId);

        Task BlockMemberAsync(int id, string userId);

        Task<LeaderboardEntryDto> InviteMemberAsync(int id, InviteMemberRequest request);

        Task<LeaderboardEntryDto> AdjustCreditsAsync(int id, string userId, AdjustCreditsRequest request);

        Task<List<RedemptionItemDto>> GetRedemptionItemsAsync(int groupId);

        Task<RedemptionItemDto> CreateRedemptionItemAsync(int groupId, CreateRedemptionItemRequest request);

        Task SetRedemptionItemActiveAsync(int groupId, int itemId, SetItemActiveRequest request);

        Task<RedemptionDto> RedeemAsync(int groupId, RedeemRequest request);

        Task<List<RedemptionDto>> GetMyRedemptionsAsync(int groupId);

        Task<List<RedemptionDto>> GetGroupRedemptionsAsync(int groupId);

        Task DeliverRedemptionAsync(int groupId, int redemptionId, DeliverRedemptionRequest request);

        Task RejectRedemptionAsync(int groupId, int redemptionId);

        Task CancelRedemptionAsync(int groupId, int redemptionId);

        Task<List<CreditTransactionDto>> GetCreditHistoryAsync(int groupId);

        Task<DuelDto> CreateDuelAsync(int groupId, CreateDuelRequest request);

        Task AcceptDuelAsync(int groupId, int duelId);

        Task DeclineDuelAsync(int groupId, int duelId);

        Task<List<DuelDto>> GetMyDuelsAsync(int groupId);

        Task<List<NotificationDto>> GetNotificationsAsync();

        Task<int> GetUnreadNotificationCountAsync();

        Task MarkNotificationReadAsync(int id);

        Task MarkAllNotificationsReadAsync();

        Task SetGroupVisibilityAsync(int id, SetVisibilityRequest request);

        Task<LeagueDto> CreateLeagueAsync(int groupId, CreateLeagueRequest request);

        Task<List<LeagueDto>> GetGroupLeaguesAsync(int groupId);

        Task<TournamentDto> CreateTournamentAsync(int groupId, CreateTournamentRequest request);

        Task<List<TournamentDto>> GetGroupTournamentsAsync(int groupId);

        Task<StandingDto> CreateTeamAsync(int groupId, CreateTeamRequest request);

        Task<MatchDto> CreateMatchAsync(int groupId, CreateMatchRequest request);

        Task<GroupDetailDto> GetGroupAsync(int id);

        Task JoinGroupAsync(int id);

        Task<GroupInviteDto> GetInviteAsync(int id);

        Task<GroupDto> JoinByCodeAsync(string code);

        Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int id);

        Task<RecapDto> GetRecapAsync(int id);

        Task<MatchDto> GetMatchAsync(int id);

        Task CloseMatchAsync(int matchId, CloseMatchRequest request);

        Task<PredictionDto> SavePredictionAsync(PredictionRequest request);

        Task<List<PredictionDto>> GetMyPredictionsAsync(int? groupId = null);

        Task<InsightsDto> GetInsightsAsync(int? groupId = null);

        Task<List<CommentDto>> GetCommentsAsync(int matchId);

        Task<CommentDto> PostCommentAsync(int matchId, string body);

        Task RegisterDeviceAsync(string token, string platform);

        Task UnregisterDeviceAsync(string token);

        Task<List<string>> GetMyDevicesAsync();
    }
}
