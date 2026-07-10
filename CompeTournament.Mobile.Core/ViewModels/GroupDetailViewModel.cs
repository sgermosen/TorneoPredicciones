namespace CompeTournament.Mobile.Core.ViewModels
{
    using System.Collections.ObjectModel;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CompeTournament.Mobile.Core.Services;
    using CompeTournament.Shared.Live;
    using CompeTournament.Shared.Tournaments;

    public partial class GroupDetailViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigation;
        private readonly ILiveTournamentClient? _live;

        public GroupDetailViewModel(IApiClient apiClient, INavigationService navigation, ILiveTournamentClient? live = null)
        {
            _apiClient = apiClient;
            _navigation = navigation;
            _live = live;
        }

        [ObservableProperty]
        private int _groupId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Title))]
        [NotifyPropertyChangedFor(nameof(CanJoin))]
        private GroupDetailDto? _group;

        [ObservableProperty]
        private string? _recap;

        [ObservableProperty]
        private string? _inviteCode;

        [ObservableProperty]
        private bool _isLive;

        public string Title => Group?.Name ?? "Grupo";

        public bool CanJoin => Group != null && !Group.IsMember;

        public ObservableCollection<MatchDto> Matches { get; } = new();

        public ObservableCollection<StandingDto> Standings { get; } = new();

        public ObservableCollection<LeaderboardEntryDto> Leaderboard { get; } = new();

        public async Task InitializeAsync(int groupId)
        {
            GroupId = groupId;
            await LoadAsync();
            await ConnectLiveAsync();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            if (IsBusy)
            {
                return;
            }

            ClearError();

            try
            {
                IsBusy = true;

                var group = await _apiClient.GetGroupAsync(GroupId);
                Group = group;

                Replace(Matches, group.Matches);
                Replace(Standings, group.Standings);

                var leaderboard = await _apiClient.GetLeaderboardAsync(GroupId);
                Replace(Leaderboard, leaderboard);

                var recap = await _apiClient.GetRecapAsync(GroupId);
                Recap = recap.Text;

                if (group.IsMember)
                {
                    try
                    {
                        var invite = await _apiClient.GetInviteAsync(GroupId);
                        InviteCode = invite.Code;
                    }
                    catch (Exception)
                    {
                        InviteCode = null;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(DescribeError(ex));
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ConnectLiveAsync()
        {
            if (_live == null)
            {
                return;
            }

            try
            {
                _live.MatchClosed += OnMatchClosed;
                await _live.JoinGroupAsync(GroupId);
                IsLive = true;
            }
            catch (Exception)
            {
                IsLive = false;
            }
        }

        private void OnMatchClosed(LiveMatchClosedDto payload)
        {
            if (payload.GroupId != GroupId)
            {
                return;
            }

            Replace(Standings, payload.Standings);
            Replace(Leaderboard, payload.Leaderboard);

            var match = Matches.FirstOrDefault(m => m.Id == payload.MatchId);
            if (match != null)
            {
                match.LocalPoints = payload.LocalPoints;
                match.VisitorPoints = payload.VisitorPoints;
                match.IsOpen = false;
            }
        }

        [RelayCommand]
        private Task OpenMatchAsync(MatchDto match)
        {
            if (match == null)
            {
                return Task.CompletedTask;
            }

            return _navigation.GoToAsync(AppRoutes.MatchPrediction, new Dictionary<string, object>
            {
                ["matchId"] = match.Id
            });
        }

        [RelayCommand]
        private async Task JoinAsync()
        {
            if (Group == null || IsBusy)
            {
                return;
            }

            ClearError();

            try
            {
                IsBusy = true;
                await _apiClient.JoinGroupAsync(GroupId);
            }
            catch (Exception ex)
            {
                ShowError(DescribeError(ex));
                return;
            }
            finally
            {
                IsBusy = false;
            }

            await LoadAsync();
        }

        private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
        {
            target.Clear();
            foreach (var item in items)
            {
                target.Add(item);
            }
        }
    }
}
