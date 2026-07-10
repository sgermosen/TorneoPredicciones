using CompeTournament.Mobile.Core.ViewModels;

namespace CompeTournament.Mobile.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfileViewModel _viewModel;

        public ProfilePage(ProfileViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.RefreshCommand.CanExecute(null))
            {
                await _viewModel.RefreshCommand.ExecuteAsync(null);
            }
        }
    }
}
