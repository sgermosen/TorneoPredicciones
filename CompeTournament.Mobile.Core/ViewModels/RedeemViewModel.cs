namespace CompeTournament.Mobile.Core.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CompeTournament.Mobile.Core.Services;
    using CompeTournament.Shared.Tournaments;

    public partial class RedeemViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigation;

        public RedeemViewModel(IApiClient apiClient, INavigationService navigation)
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

        [ObservableProperty]
        private string? _successMessage;

        [ObservableProperty]
        private string _deliveryInfo = string.Empty;

        public string Title => GroupName ?? "Premios";

        public ObservableCollection<RedemptionItemDto> Items { get; } = new();

        public ObservableCollection<RedemptionDto> MyRedemptions { get; } = new();

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

                var items = await _apiClient.GetRedemptionItemsAsync(GroupId);
                Replace(Items, items);

                var mine = await _apiClient.GetMyRedemptionsAsync(GroupId);
                Replace(MyRedemptions, mine);
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
        private async Task RedeemAsync(RedemptionItemDto item)
        {
            if (item == null || IsBusy)
            {
                return;
            }

            var info = string.IsNullOrWhiteSpace(DeliveryInfo) ? null : DeliveryInfo.Trim();
            await RunAsync(
                () => _apiClient.RedeemAsync(GroupId, new RedeemRequest { RedemptionItemId = item.Id, DeliveryInfo = info }),
                $"Canjeaste {item.Name}.");
            DeliveryInfo = string.Empty;
        }

        [RelayCommand]
        private async Task CancelAsync(RedemptionDto redemption)
        {
            if (redemption == null || IsBusy)
            {
                return;
            }

            await RunAsync(
                () => _apiClient.CancelRedemptionAsync(GroupId, redemption.Id),
                "Canje cancelado y fichas devueltas.");
        }

        [RelayCommand]
        private Task BackAsync() => _navigation.GoBackAsync();

        private async Task RunAsync(Func<Task> action, string success)
        {
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
