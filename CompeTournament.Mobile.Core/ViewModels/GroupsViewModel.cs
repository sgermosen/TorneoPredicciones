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

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private string _inviteCode = string.Empty;

        [ObservableProperty]
        private string? _successMessage;

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
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task JoinByCodeAsync()
        {
            if (IsBusy)
            {
                return;
            }

            ClearError();
            SuccessMessage = null;

            if (string.IsNullOrWhiteSpace(InviteCode))
            {
                ShowError("Escribe el código de invitación.");
                return;
            }

            GroupDto group;
            try
            {
                IsBusy = true;
                group = await _apiClient.JoinByCodeAsync(InviteCode.Trim());
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

            InviteCode = string.Empty;
            await LoadAsync();
            SuccessMessage = $"Te uniste a {group.Name}.";
        }

        [RelayCommand]
        private Task OpenCreateAsync() => _navigation.GoToAsync(AppRoutes.CreateGroup);

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
