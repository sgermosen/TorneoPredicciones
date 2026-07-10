namespace CompeTournament.Mobile.Core.ViewModels
{
    using System.Collections.ObjectModel;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CompeTournament.Mobile.Core.Services;
    using CompeTournament.Shared.Tournaments;

    public partial class GroupDetailViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigation;

        public GroupDetailViewModel(IApiClient apiClient, INavigationService navigation)
        {
            _apiClient = apiClient;
            _navigation = navigation;
        }

        [ObservableProperty]
        private int _groupId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Title))]
        [NotifyPropertyChangedFor(nameof(CanJoin))]
        private GroupDetailDto? _group;

        public string Title => Group?.Name ?? "Grupo";

        public bool CanJoin => Group != null && !Group.IsMember;

        public ObservableCollection<MatchDto> Matches { get; } = new();

        public ObservableCollection<StandingDto> Standings { get; } = new();

        public ObservableCollection<LeaderboardEntryDto> Leaderboard { get; } = new();

        public async Task InitializeAsync(int groupId)
        {
            GroupId = groupId;
            await LoadAsync();
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

                Matches.Clear();
                foreach (var match in group.Matches)
                {
                    Matches.Add(match);
                }

                Standings.Clear();
                foreach (var standing in group.Standings)
                {
                    Standings.Add(standing);
                }

                Leaderboard.Clear();
                var leaderboard = await _apiClient.GetLeaderboardAsync(GroupId);
                foreach (var entry in leaderboard)
                {
                    Leaderboard.Add(entry);
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

        [RelayCommand]
        private Task OpenMatchAsync(MatchDto match)
        {
            if (match == null || !match.IsOpen)
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
    }
}
