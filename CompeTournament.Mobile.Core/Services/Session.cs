namespace CompeTournament.Mobile.Core.Services
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CompeTournament.Shared.Auth;

    public partial class Session : ObservableObject, ISession
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAuthenticated))]
        private UserDto? _currentUser;

        public bool IsAuthenticated => CurrentUser != null;

        public void SetUser(UserDto user) => CurrentUser = user;

        public void UpdatePoints(int points)
        {
            if (CurrentUser != null)
            {
                CurrentUser.Points = points;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }

        public void Clear() => CurrentUser = null;
    }
}
