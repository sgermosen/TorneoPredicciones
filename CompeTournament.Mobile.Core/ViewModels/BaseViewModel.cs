namespace CompeTournament.Mobile.Core.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CompeTournament.Mobile.Core.Services;

    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        [ObservableProperty]
        private string? _errorMessage;

        public bool IsNotBusy => !IsBusy;

        protected void ClearError() => ErrorMessage = null;

        protected void ShowError(string message) => ErrorMessage = message;

        protected string DescribeError(Exception exception) =>
            exception is ApiException api ? api.Message : "Ocurrio un error inesperado.";
    }
}
