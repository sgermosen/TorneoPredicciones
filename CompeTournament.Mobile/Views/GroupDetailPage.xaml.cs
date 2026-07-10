using CompeTournament.Mobile.Core.ViewModels;

namespace CompeTournament.Mobile.Views
{
    [QueryProperty(nameof(GroupId), "groupId")]
    public partial class GroupDetailPage : ContentPage
    {
        private readonly GroupDetailViewModel _viewModel;

        public GroupDetailPage(GroupDetailViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        public int GroupId { get; set; }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.Group == null)
            {
                await _viewModel.InitializeAsync(GroupId);
            }
        }
    }
}
