namespace CompeTournament.Mobile.Core.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CompeTournament.Mobile.Core.Services;
    using CompeTournament.Shared.Notifications;

    public partial class NotificationsViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigation;

        public NotificationsViewModel(IApiClient apiClient, INavigationService navigation)
        {
            _apiClient = apiClient;
            _navigation = navigation;
        }

        [ObservableProperty]
        private bool _isRefreshing;

        public ObservableCollection<NotificationDto> Notifications { get; } = new();

        public async Task InitializeAsync()
        {
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
                var notifications = await _apiClient.GetNotificationsAsync();
                Replace(Notifications, notifications);
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
        private async Task MarkReadAsync(NotificationDto notification)
        {
            if (notification == null || notification.IsRead || IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                await _apiClient.MarkNotificationReadAsync(notification.Id);
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

        [RelayCommand]
        private async Task MarkAllReadAsync()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                await _apiClient.MarkAllNotificationsReadAsync();
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
