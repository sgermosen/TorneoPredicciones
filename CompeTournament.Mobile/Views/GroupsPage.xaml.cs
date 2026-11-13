using CompeTournament.Mobile.Core.ViewModels;

namespace CompeTournament.Mobile.Views
{
    public partial class GroupsPage : ContentPage
    {
        private readonly GroupsViewModel _viewModel;

        public GroupsPage(GroupsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
            _viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(GroupsViewModel.SuccessMessage) && _viewModel.SuccessMessage != null)
                {
                    InviteCodeEntry.Unfocus();
                }
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
            await _viewModel.TryJoinPendingAsync();
        }

        protected override void OnDisappearing()
        {
            InviteCodeEntry.Unfocus();
            base.OnDisappearing();
        }
    }
}
