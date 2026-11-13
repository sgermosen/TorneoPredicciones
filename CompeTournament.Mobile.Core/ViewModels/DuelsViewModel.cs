namespace CompeTournament.Mobile.Core.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CompeTournament.Mobile.Core.Services;
    using CompeTournament.Shared.Tournaments;

    public class DuelMatchOption
    {
        public int MatchId { get; set; }

        public string Label { get; set; }
    }

    public partial class DuelsViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigation;
        private readonly ISession _session;

        public DuelsViewModel(IApiClient apiClient, INavigationService navigation, ISession session)
        {
            _apiClient = apiClient;
            _navigation = navigation;
            _session = session;
        }

        [ObservableProperty]
        private int _groupId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Title))]
        private string? _groupName;

        [ObservableProperty]
        private string? _successMessage;

        [ObservableProperty]
        private LeaderboardEntryDto? _selectedOpponent;

        [ObservableProperty]
        private DuelMatchOption? _selectedMatch;

        public string Title => GroupName ?? "Duelos";

        public ObservableCollection<LeaderboardEntryDto> Opponents { get; } = new();

        public ObservableCollection<DuelMatchOption> Matches { get; } = new();

        public ObservableCollection<DuelDto> Duels { get; } = new();

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
                GroupName = group.Name;

                var myId = _session.CurrentUser?.Id;
                var leaderboard = await _apiClient.GetLeaderboardAsync(GroupId);
                Replace(Opponents, leaderboard.Where(e => e.UserId != myId));

                Replace(Matches, group.Matches.Select(m => new DuelMatchOption
                {
                    MatchId = m.Id,
                    Label = $"{m.LocalInitials} vs {m.VisitorInitials}"
                }));

                var duels = await _apiClient.GetMyDuelsAsync(GroupId);
                Replace(Duels, duels);
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
        private async Task CreateAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (SelectedOpponent == null || SelectedMatch == null)
            {
                ShowError("Elige un rival y un partido.");
                return;
            }

            await RunAsync(
                () => _apiClient.CreateDuelAsync(GroupId, new CreateDuelRequest
                {
                    OpponentUserId = SelectedOpponent.UserId,
                    MatchId = SelectedMatch.MatchId
                }),
                $"Retaste a {SelectedOpponent.FullName}.");
        }

        [RelayCommand]
        private async Task AcceptAsync(DuelDto duel)
        {
            if (duel == null)
            {
                return;
            }

            await RunAsync(() => _apiClient.AcceptDuelAsync(GroupId, duel.Id), "Duelo aceptado.");
        }

        [RelayCommand]
        private async Task DeclineAsync(DuelDto duel)
        {
            if (duel == null)
            {
                return;
            }

            await RunAsync(() => _apiClient.DeclineDuelAsync(GroupId, duel.Id), "Duelo rechazado.");
        }

        [RelayCommand]
        private Task BackAsync() => _navigation.GoBackAsync();

        private async Task RunAsync(Func<Task> action, string success)
        {
            if (IsBusy)
            {
                return;
            }

            ClearError();
            SuccessMessage = null;

            try
            {
                IsBusy = true;
                await action();
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
            SuccessMessage = success;
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
