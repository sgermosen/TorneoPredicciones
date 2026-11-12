namespace CompeTournament.Backend.Data
{
    using CompeTournament.Backend.Data.Entities;
    using Helpers;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class SeedDb
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly Random _random;

        public SeedDb(ApplicationDbContext context, IUserHelper userHelper, IConfiguration configuration, IHostEnvironment environment)
        {
            _context = context;
            _userHelper = userHelper;
            _configuration = configuration;
            _environment = environment;
            _random = new Random();
        }

        public async Task SeedAsync()
        {
               await _context.Database.EnsureCreatedAsync();

            if (!_context.Statuses.Any())
            {
                AddStatus("Pendiente de iniciar");
                AddStatus("Cerrado");
                AddStatus("Terminado");
                AddStatus("Cancelado");
                await _context.SaveChangesAsync();
            }

            if (!_context.TournamentTypes.Any())
            {
                AddTounamentType("Por marcador (fútbol, béisbol, baloncesto)");
                AddTounamentType("Un solo ganador (boxeo, parchís, votaciones)");
                AddTounamentType("Por posiciones (ciclismo, natación, lotería)");
                await _context.SaveChangesAsync();
            }

            if (!_context.UserTypes.Any())
            {
                AddUserType("Local");
                AddUserType("GitHub");
                AddUserType("Facebook");
                AddUserType("Twitter");
                AddUserType("Instagram");
                AddUserType("Microsoft");
                AddUserType("Google");
                AddUserType("Other");
                await _context.SaveChangesAsync();
            }

            await CheckRolesAsync();
            await CheckUser("jugador1@example.com", "Ana", "Pérez", "User");
            await CheckUser("jugador2@example.com", "Luis", "Gómez", "User");
            await CheckUser("organizador@example.com", "Carla", "Díaz", "Admin");
            var admin = await CheckUser("admin@example.com", "Administrador", "Copa Amistosa", "Admin");

            await _context.SaveChangesAsync();

            await SeedDemoTournamentAsync(admin);
        }

        private async Task SeedDemoTournamentAsync(ApplicationUser owner)
        {
            if (_context.Groups.Any())
            {
                return;
            }

            var tournamentType = _context.TournamentTypes.First();

            var group = new Group
            {
                Name = "Copa Amistosa 2026",
                Requirements = "Reta a tus amigos a predecir los partidos de la copa.",
                InviteCode = "COPA2026",
                Active = true,
                IsPublic = true,
                TournamentTypeId = tournamentType.Id
            };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var league = new League
            {
                Name = "Liga Nacional",
                Active = true,
                GroupId = group.Id
            };
            _context.Leagues.Add(league);
            await _context.SaveChangesAsync();

            var tigres = AddTeam("Tigres", "TIG", league.Id);
            var leones = AddTeam("Leones", "LEO", league.Id);
            var aguilas = AddTeam("Águilas", "AGU", league.Id);
            var toros = AddTeam("Toros", "TOR", league.Id);
            await _context.SaveChangesAsync();

            AddMatch(group.Id, tigres.Id, leones.Id, 2);
            AddMatch(group.Id, aguilas.Id, toros.Id, 4);
            await _context.SaveChangesAsync();

            _context.GroupUsers.Add(new GroupUser
            {
                GroupId = group.Id,
                ApplicationUserId = owner.Id,
                IsAccepted = true,
                IsBlocked = false,
                Points = 0
            });
            await _context.SaveChangesAsync();
        }

        private Team AddTeam(string name, string initials, int leagueId)
        {
            var team = new Team
            {
                Name = name,
                Initials = initials,
                Active = true,
                LeagueId = leagueId,
                MatchesPlayed = 0,
                MatchesWon = 0,
                MatchesTied = 0,
                MatchesLost = 0,
                FavorPoints = 0,
                AgainstPoints = 0,
                CumulativePoints = 0,
                Position = 0
            };
            _context.Teams.Add(team);
            return team;
        }

        private void AddMatch(int groupId, int localId, int visitorId, int daysFromNow)
        {
            _context.Matches.Add(new Match
            {
                Name = "Partido",
                Active = true,
                GroupId = groupId,
                LocalId = localId,
                VisitorId = visitorId,
                DateTime = DateTime.UtcNow.AddDays(daysFromNow),
                StatusId = 1
            });
        }

        private void AddStatus(string v)
        {
            _context.Statuses.Add(new Status
            {
                Name = v
            });
        }

        private void AddTounamentType(string v)
        {
            _context.TournamentTypes.Add(new TournamentType
            {
                Name = v
            });
        }

        private void AddUserType(string v)
        {
            _context.UserTypes.Add(new UserType
            {
                Name = v
            });
        }

        //private void AddPaymentType(string v1, string v2)
        //{
        //    _context.PaymentTypes.Add(new PaymentType
        //    {
        //        Code = v1,
        //        Name = v2,
        //    });
        //}



        //private void AddCountry(string name, string denomyn)
        //{
        //    _context.Countries.Add(new Country
        //    {
        //        Name = name,
        //        Denomym = denomyn
        //    });
        //}


        private async Task<ApplicationUser> CheckUser(string userName, string firstName, string lastName, string role)
        {
            // Add user
            var user = await _userHelper.GetUserByEmailAsync(userName);
            if (user == null)
            {
                user = await AddUser(userName, firstName, lastName, role);
            }

            var isInRole = await _userHelper.IsUserInRoleAsync(user, role);
            if (!isInRole)
            {
                await _userHelper.AddUserToRoleAsync(user, role);
            }

            return user;
        }

        private async Task<ApplicationUser> AddUser(string userName, string firstName, string lastName, string role)
        {
            var user = new ApplicationUser
            {
                Name = firstName,
                Lastname = lastName,
                Email = userName,
                UserName = userName,
                PhoneNumber = "809 555 0100",
                UserType= _context.UserTypes.FirstOrDefault() 
                //CityId = context.Countries.FirstOrDefault().Cities.FirstOrDefault().Id,
                //City = context.Countries.FirstOrDefault().Cities.FirstOrDefault()
            };

            var seedPassword = _configuration["Seed:DefaultPassword"];
            if (string.IsNullOrWhiteSpace(seedPassword))
            {
                if (!_environment.IsDevelopment())
                {
                    throw new InvalidOperationException(
                        "Seed:DefaultPassword no esta configurada. Defina la clave de los usuarios de demo via variables de entorno antes de ejecutar fuera de Development.");
                }

                seedPassword = "Torneo2026";
            }
            var result = await _userHelper.AddUserAsync(user, seedPassword);
            if (result != IdentityResult.Success)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Could not create the user in seeder: {errors}");
            }
          // _userHelper.AddClaim(user, new Claim("OwnerId", owner.Id.ToString()));

            await _userHelper.AddUserToRoleAsync(user, role);
            var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
            await _userHelper.ConfirmEmailAsync(user, token);
            return user;
        }

        private async Task CheckRolesAsync()
        {
            await _userHelper.CheckRoleAsync("Admin");
            await _userHelper.CheckRoleAsync("User");
        }


    }
}
