using CompeTournament.Mobile.Core.ViewModels;

namespace CompeTournament.Mobile.Views
{
    [QueryProperty(nameof(MatchId), "matchId")]
    public partial class MatchPredictionPage : ContentPage
    {
        private readonly MatchPredictionViewModel _viewModel;

        public MatchPredictionPage(MatchPredictionViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        public int MatchId { get; set; }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.Match == null)
            {
                await _viewModel.InitializeAsync(MatchId);
            }
        }
    }
}
