namespace CompeTournament.Mobile.Core.ViewModels
{
    using System.Collections.ObjectModel;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CompeTournament.Mobile.Core.Services;
    using CompeTournament.Shared.Tournaments;

    public partial class MatchPredictionViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigation;

        public MatchPredictionViewModel(IApiClient apiClient, INavigationService navigation)
        {
            _apiClient = apiClient;
            _navigation = navigation;
        }

        public ObservableCollection<CommentDto> Comments { get; } = new();

        [ObservableProperty]
        private string _newCommentBody = string.Empty;

        [ObservableProperty]
        private int _matchId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanPredict))]
        private MatchDto? _match;

        [ObservableProperty]
        private int _localPoints;

        [ObservableProperty]
        private int _visitorPoints;

        [ObservableProperty]
        private bool _isBanker;

        [ObservableProperty]
        private string? _successMessage;

        public bool CanPredict => Match?.IsOpen == true;

        public async Task InitializeAsync(int matchId)
        {
            MatchId = matchId;
            await LoadAsync();
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy)
            {
                return;
            }

            ClearError();

            try
            {
                IsBusy = true;
                var match = await _apiClient.GetMatchAsync(MatchId);
                Match = match;

                if (match.MyPrediction != null)
                {
                    LocalPoints = match.MyPrediction.LocalPoints ?? 0;
                    VisitorPoints = match.MyPrediction.VisitorPoints ?? 0;
                    IsBanker = match.MyPrediction.IsBanker;
                }

                Comments.Clear();
                var comments = await _apiClient.GetCommentsAsync(MatchId);
                foreach (var comment in comments)
                {
                    Comments.Add(comment);
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
        private void IncrementLocal() => LocalPoints++;

        [RelayCommand]
        private void DecrementLocal()
        {
            if (LocalPoints > 0)
            {
                LocalPoints--;
            }
        }

        [RelayCommand]
        private void IncrementVisitor() => VisitorPoints++;

        [RelayCommand]
        private void DecrementVisitor()
        {
            if (VisitorPoints > 0)
            {
                VisitorPoints--;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IsBusy || !CanPredict)
            {
                return;
            }

            ClearError();
            SuccessMessage = null;

            try
            {
                IsBusy = true;
                await _apiClient.SavePredictionAsync(new PredictionRequest
                {
                    MatchId = MatchId,
                    LocalPoints = LocalPoints,
                    VisitorPoints = VisitorPoints,
                    IsBanker = IsBanker
                });

                SuccessMessage = "Prediccion guardada.";
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
        private async Task PostCommentAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCommentBody))
            {
                return;
            }

            ClearError();

            try
            {
                var comment = await _apiClient.PostCommentAsync(MatchId, NewCommentBody.Trim());
                Comments.Add(comment);
                NewCommentBody = string.Empty;
            }
            catch (Exception ex)
            {
                ShowError(DescribeError(ex));
            }
        }
    }
}
