namespace CompeTournament.Backend.Data
{
    using CompeTournament.Backend.Data.Entities;
    using Helpers;
    using Microsoft.AspNetCore.Identity;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class SeedDb
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserHelper _userHelper;
        private readonly Random _random;

        public SeedDb(ApplicationDbContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _random = new Random();
        }

        public async Task SeedAsync()
        {
               await _context.Database.EnsureCreatedAsync();

            if (!_context.Statuses.Any())
            {
                AddStatus("Pendiente de Iniciar");
                AddStatus("Cerrado");
                AddStatus("Terminado");
                AddStatus("Cancelado");
                await _context.SaveChangesAsync();
            }

            if (!_context.TournamentTypes.Any())
            {
                AddTounamentType("Point Match (ej:Soccer, BaseBall, BasketBall)");
                AddTounamentType("Only One Winner (ej:Box, Parchis, Votation)");
                AddTounamentType("Position Match (ej:Bike, Swimming, Lotery)");
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
            await CheckUser("elis@gmail.com", "Elis", "Pascual", "User");
            await CheckUser("starling@gmail.com", "Starling", "Germosen", "User");
            await CheckUser("toreneo@gmail.com", "Mersy", "RD", "Admin");
            await CheckUser("sgrysoft@gmail.com", "Starling", "Germosen", "Admin");

            await _context.SaveChangesAsync();

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
                PhoneNumber = "849 207 7714",
                UserType= _context.UserTypes.FirstOrDefault() 
                //CityId = context.Countries.FirstOrDefault().Cities.FirstOrDefault().Id,
                //City = context.Countries.FirstOrDefault().Cities.FirstOrDefault()
            };

            var result = await _userHelper.AddUserAsync(user, "123456");
            if (result != IdentityResult.Success)
            {
                throw new InvalidOperationException("Could not create the user in seeder");
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
