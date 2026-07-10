namespace CompeTournament.Mobile.Core.ViewModels
{
    using System.Collections.ObjectModel;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CompeTournament.Mobile.Core.Services;
    using CompeTournament.Shared.Tournaments;

    public partial class GroupsViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigation;

        public GroupsViewModel(IApiClient apiClient, INavigationService navigation)
        {
            _apiClient = apiClient;
            _navigation = navigation;
        }

        public ObservableCollection<GroupDto> Groups { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Title))]
        private bool _showOnlyMine;

        public string Title => ShowOnlyMine ? "Mis grupos" : "Torneos";

        partial void OnShowOnlyMineChanged(bool value) => _ = LoadAsync();

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
                var groups = ShowOnlyMine
                    ? await _apiClient.GetMyGroupsAsync()
                    : await _apiClient.GetGroupsAsync();

                Groups.Clear();
                foreach (var group in groups)
                {
                    Groups.Add(group);
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
        private Task OpenGroupAsync(GroupDto group)
        {
            if (group == null)
            {
                return Task.CompletedTask;
            }

            return _navigation.GoToAsync(AppRoutes.GroupDetail, new Dictionary<string, object>
            {
                ["groupId"] = group.Id
            });
        }

        [RelayCommand]
        private async Task JoinAsync(GroupDto group)
        {
            if (group == null || IsBusy)
            {
                return;
            }

            ClearError();

            try
            {
                IsBusy = true;
                await _apiClient.JoinGroupAsync(group.Id);
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
