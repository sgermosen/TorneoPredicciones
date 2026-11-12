namespace CompeTournament.Mobile.Core.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CompeTournament.Mobile.Core.Services;
    using CompeTournament.Shared.Tournaments;

    public partial class CreateGroupViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigation;

        public CreateGroupViewModel(IApiClient apiClient, INavigationService navigation)
        {
            _apiClient = apiClient;
            _navigation = navigation;
        }

        public ObservableCollection<TournamentTypeDto> TournamentTypes { get; } = new();

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _requirements = string.Empty;

        [ObservableProperty]
        private TournamentTypeDto? _selectedType;

        [ObservableProperty]
        private bool _isPublic = true;

        public async Task InitializeAsync()
        {
            if (IsBusy)
            {
                return;
            }

            ClearError();

            try
            {
                IsBusy = true;
                var types = await _apiClient.GetTournamentTypesAsync();
                TournamentTypes.Clear();
                foreach (var type in types)
                {
                    TournamentTypes.Add(type);
                }

                SelectedType ??= TournamentTypes.FirstOrDefault();
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

            ClearError();

            if (string.IsNullOrWhiteSpace(Name))
            {
                ShowError("Escribe el nombre del torneo.");
                return;
            }

            if (SelectedType == null)
            {
                ShowError("Elige un tipo de torneo.");
                return;
            }

            try
            {
                IsBusy = true;
                await _apiClient.CreateGroupAsync(new CreateGroupRequest
                {
                    Name = Name.Trim(),
                    Requirements = string.IsNullOrWhiteSpace(Requirements) ? null : Requirements.Trim(),
                    TournamentTypeId = SelectedType.Id,
                    IsPublic = IsPublic
                });

                await _navigation.GoBackAsync();
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
        private Task CancelAsync() => _navigation.GoBackAsync();
    }
}
