namespace CompeTournament.Mobile.Core.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CompeTournament.Mobile.Core.Services;
    using CompeTournament.Shared.Tournaments;

    public partial class GroupManageViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigation;

        public GroupManageViewModel(IApiClient apiClient, INavigationService navigation)
        {
            _apiClient = apiClient;
            _navigation = navigation;
        }

        [ObservableProperty]
        private int _groupId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Title))]
        private string? _groupName;

        [ObservableProperty]
        private string? _successMessage;

        [ObservableProperty]
        private bool _isPublic;

        [ObservableProperty]
        private bool _creditsEnabled;

        [ObservableProperty]
        private int _myCredits;

        [ObservableProperty]
        private string _inviteEmail = string.Empty;

        public string Title => GroupName ?? "Gestionar";

        public ObservableCollection<LeaderboardEntryDto> Members { get; } = new();

        public ObservableCollection<LeagueDto> Leagues { get; } = new();

        public ObservableCollection<TournamentDto> Tournaments { get; } = new();

        public ObservableCollection<StandingDto> Teams { get; } = new();

        public ObservableCollection<MatchDto> OpenMatches { get; } = new();

        [ObservableProperty]
        private string _newLeagueName = string.Empty;

        [ObservableProperty]
        private LeagueDto? _selectedLeague;

        [ObservableProperty]
        private string _newTournamentName = string.Empty;

        [ObservableProperty]
        private TournamentDto? _selectedTournament;

        [ObservableProperty]
        private string _newTeamName = string.Empty;

        [ObservableProperty]
        private string _newMatchName = string.Empty;

        [ObservableProperty]
        private StandingDto? _selectedLocal;

        [ObservableProperty]
        private StandingDto? _selectedVisitor;

        [ObservableProperty]
        private DateTime _matchDate = DateTime.Now.Date.AddDays(1);

        [ObservableProperty]
        private TimeSpan _matchTime = new TimeSpan(18, 0, 0);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasMatchToClose))]
        [NotifyPropertyChangedFor(nameof(CloseTitle))]
        private MatchDto? _selectedMatchToClose;

        [ObservableProperty]
        private string _closeLocalPoints = string.Empty;

        [ObservableProperty]
        private string _closeVisitorPoints = string.Empty;

        public bool HasMatchToClose => SelectedMatchToClose != null;

        public string CloseTitle => SelectedMatchToClose == null
            ? string.Empty
            : $"Marcador final: {SelectedMatchToClose.LocalInitials} vs {SelectedMatchToClose.VisitorInitials}";

        public async Task InitializeAsync(int groupId)
        {
            GroupId = groupId;
            await LoadAsync();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            if (IsBusy)
            {
                return;
            }

            ClearError();

            try
            {
                IsBusy = true;

                var group = await _apiClient.GetGroupAsync(GroupId);
                GroupName = group.Name;
                IsPublic = group.IsPublic;
                CreditsEnabled = group.CreditsEnabled;
                MyCredits = group.MyCredits;
                Replace(Teams, group.Standings);
                Replace(OpenMatches, group.Matches.Where(m => m.IsOpen));

                var members = await _apiClient.GetGroupMembersAsync(GroupId);
                Replace(Members, members);

                var leagues = await _apiClient.GetGroupLeaguesAsync(GroupId);
                Replace(Leagues, leagues);
                SelectedLeague ??= Leagues.FirstOrDefault();

                var tournaments = await _apiClient.GetGroupTournamentsAsync(GroupId);
                Replace(Tournaments, tournaments);
            }
            catch (Exception ex)
            {
                ShowError(DescribeError(ex));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AcceptMemberAsync(LeaderboardEntryDto member)
        {
            if (member == null || IsBusy)
            {
                return;
            }

            await RunAsync(() => _apiClient.AcceptMemberAsync(GroupId, member.UserId), $"{Describe(member)} aceptado.");
        }

        [RelayCommand]
        private async Task BlockMemberAsync(LeaderboardEntryDto member)
        {
            if (member == null || IsBusy)
            {
                return;
            }

            await RunAsync(() => _apiClient.BlockMemberAsync(GroupId, member.UserId), $"{Describe(member)} bloqueado.");
        }

        [RelayCommand]
        private async Task CreateLeagueAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(NewLeagueName))
            {
                ShowError("Escribe el nombre de la liga.");
                return;
            }

            var name = NewLeagueName.Trim();
            await RunAsync(() => _apiClient.CreateLeagueAsync(GroupId, new CreateLeagueRequest { Name = name }), $"Liga {name} creada.");
            NewLeagueName = string.Empty;
        }

        [RelayCommand]
        private async Task CreateTournamentAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(NewTournamentName))
            {
                ShowError("Escribe el nombre del torneo interno.");
                return;
            }

            var name = NewTournamentName.Trim();
            await RunAsync(() => _apiClient.CreateTournamentAsync(GroupId, new CreateTournamentRequest { Name = name }), $"Torneo {name} creado.");
            NewTournamentName = string.Empty;
        }

        [RelayCommand]
        private async Task CreateTeamAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (SelectedLeague == null)
            {
                ShowError("Elige una liga.");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewTeamName))
            {
                ShowError("Escribe el nombre del equipo.");
                return;
            }

            var name = NewTeamName.Trim();
            var leagueId = SelectedLeague.Id;
            await RunAsync(() => _apiClient.CreateTeamAsync(GroupId, new CreateTeamRequest { LeagueId = leagueId, Name = name }), $"Equipo {name} creado.");
            NewTeamName = string.Empty;
        }

        [RelayCommand]
        private async Task CreateMatchAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (SelectedLocal == null || SelectedVisitor == null)
            {
                ShowError("Elige el equipo local y el visitante.");
                return;
            }

            if (SelectedLocal.TeamId == SelectedVisitor.TeamId)
            {
                ShowError("El local y el visitante deben ser distintos.");
                return;
            }

            var kickoff = DateTime.SpecifyKind(MatchDate.Date + MatchTime, DateTimeKind.Local).ToUniversalTime();
            var request = new CreateMatchRequest
            {
                Name = string.IsNullOrWhiteSpace(NewMatchName) ? null : NewMatchName.Trim(),
                LocalId = SelectedLocal.TeamId,
                VisitorId = SelectedVisitor.TeamId,
                DateTime = kickoff,
                TournamentId = SelectedTournament?.Id
            };

            await RunAsync(() => _apiClient.CreateMatchAsync(GroupId, request), "Partido creado.");
            NewMatchName = string.Empty;
        }

        [RelayCommand]
        private void SelectMatchToClose(MatchDto match)
        {
            ClearError();
            SuccessMessage = null;
            SelectedMatchToClose = match;
            CloseLocalPoints = string.Empty;
            CloseVisitorPoints = string.Empty;
        }

        [RelayCommand]
        private async Task CloseMatchAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (SelectedMatchToClose == null)
            {
                ShowError("Elige el partido a cerrar.");
                return;
            }

            if (!int.TryParse(CloseLocalPoints, out var local) || !int.TryParse(CloseVisitorPoints, out var visitor)
                || local < 0 || visitor < 0)
            {
                ShowError("Escribe un marcador válido (números de 0 o más).");
                return;
            }

            var matchId = SelectedMatchToClose.Id;
            await RunAsync(() => _apiClient.CloseMatchAsync(matchId, new CloseMatchRequest { LocalPoints = local, VisitorPoints = visitor }), "Partido cerrado.");
            SelectedMatchToClose = null;
            CloseLocalPoints = string.Empty;
            CloseVisitorPoints = string.Empty;
        }

        [RelayCommand]
        private async Task InviteAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(InviteEmail))
            {
                ShowError("Escribe el correo de la persona.");
                return;
            }

            var email = InviteEmail.Trim();
            await RunAsync(() => _apiClient.InviteMemberAsync(GroupId, new InviteMemberRequest { Email = email }), "Invitación enviada.");
            InviteEmail = string.Empty;
        }

        [RelayCommand]
        private async Task ToggleVisibilityAsync()
        {
            if (IsBusy)
            {
                return;
            }

            var target = !IsPublic;
            await RunAsync(
                () => _apiClient.SetGroupVisibilityAsync(GroupId, new SetVisibilityRequest { IsPublic = target }),
                target ? "El torneo ahora es público." : "El torneo ahora es privado.");
        }

        [RelayCommand]
        private Task BackAsync() => _navigation.GoBackAsync();

        private async Task RunAsync(Func<Task> action, string success)
        {
            ClearError();
            SuccessMessage = null;

            try
            {
                IsBusy = true;
                await action();
            }
            catch (Exception ex)
            {
                ShowError(DescribeError(ex));
                return;
            }
            finally
            {
                IsBusy = false;
            }

            await LoadAsync();
            SuccessMessage = success;
        }

        private static string Describe(LeaderboardEntryDto member) =>
            string.IsNullOrWhiteSpace(member.FullName) ? "El jugador" : member.FullName;

        private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
        {
            target.Clear();
            foreach (var item in items)
            {
                target.Add(item);
            }
        }
    }
}
