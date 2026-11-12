using CompeTournament.Mobile.Core.ViewModels;

namespace CompeTournament.Mobile.Views
{
    public partial class CreateGroupPage : ContentPage
    {
        private readonly CreateGroupViewModel _viewModel;

        public CreateGroupPage(CreateGroupViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.TournamentTypes.Count == 0)
            {
                await _viewModel.InitializeAsync();
            }
        }
    }
}
