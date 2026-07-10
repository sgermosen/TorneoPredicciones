namespace CompeTournament.Mobile.Core.Services
{
    using CompeTournament.Shared.Auth;

    public interface ISession
    {
        UserDto? CurrentUser { get; }

        bool IsAuthenticated { get; }

        void SetUser(UserDto user);

        void UpdatePoints(int points);

        void Clear();
    }
}
