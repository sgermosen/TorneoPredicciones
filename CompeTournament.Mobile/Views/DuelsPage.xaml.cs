using CompeTournament.Mobile.Core.ViewModels;

namespace CompeTournament.Mobile.Views
{
    [QueryProperty(nameof(GroupId), "groupId")]
    public partial class DuelsPage : ContentPage
    {
        private readonly DuelsViewModel _viewModel;

        public DuelsPage(DuelsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        public int GroupId { get; set; }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.GroupId == 0)
            {
                await _viewModel.InitializeAsync(GroupId);
            }
            else
            {
                await _viewModel.LoadAsync();
            }
        }
    }
}
