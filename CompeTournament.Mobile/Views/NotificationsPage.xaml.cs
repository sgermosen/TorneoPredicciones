using CompeTournament.Mobile.Core.ViewModels;

namespace CompeTournament.Mobile.Views
{
    public partial class NotificationsPage : ContentPage
    {
        private readonly NotificationsViewModel _viewModel;

        public NotificationsPage(NotificationsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}
