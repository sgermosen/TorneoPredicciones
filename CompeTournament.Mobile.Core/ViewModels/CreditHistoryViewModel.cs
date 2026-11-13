namespace CompeTournament.Mobile.Core.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CompeTournament.Mobile.Core.Services;
    using CompeTournament.Shared.Tournaments;

    public partial class CreditHistoryViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigation;

        public CreditHistoryViewModel(IApiClient apiClient, INavigationService navigation)
        {
            _apiClient = apiClient;
            _navigation = navigation;
        }

        [ObservableProperty]
        private int _groupId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Title))]
        private string? _groupName;

        [ObservableProperty]
        private int _myCredits;

        public string Title => GroupName ?? "Historial de fichas";

        public ObservableCollection<CreditTransactionDto> History { get; } = new();

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
                MyCredits = group.MyCredits;

                var history = await _apiClient.GetCreditHistoryAsync(GroupId);
                Replace(History, history);
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
        private Task BackAsync() => _navigation.GoBackAsync();

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
