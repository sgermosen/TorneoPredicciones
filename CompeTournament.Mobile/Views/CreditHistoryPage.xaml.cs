using CompeTournament.Mobile.Core.ViewModels;

namespace CompeTournament.Mobile.Views
{
    [QueryProperty(nameof(GroupId), "groupId")]
    public partial class CreditHistoryPage : ContentPage
    {
        private readonly CreditHistoryViewModel _viewModel;

        public CreditHistoryPage(CreditHistoryViewModel viewModel)
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
