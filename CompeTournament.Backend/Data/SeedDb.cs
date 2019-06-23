namespace CompeTournament.Backend.Data
{
    using Helpers;
    using Microsoft.AspNetCore.Identity;
    using System;
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
          //  await _context.Database.EnsureCreatedAsync();

            //if (!_context.Countries.Any())
            //{
            //    AddCountry("Republica Dominicana", "_Dominican@");
            //    AddCountry("Venezuela", "Venezolan@");
            //    AddCountry("China", "Chino@");
            //    AddCountry("Colombia", "Colombian@");
            //    AddCountry("Haiti", "Haitian@");
            //    AddCountry("Usa", "American@");
            //    AddCountry("Otro", "Otro");
            //}
            await CheckRolesAsync();
            await CheckUser("elis@gmail.com", "Elis", "Pascual", "User");
            await CheckUser("starling@gmail.com", "Starling", "Germosen", "User");
            await CheckUser("toreneo@gmail.com", "Mersy", "RD", "Admin");
            await CheckUser("sgrysoft@gmail.com", "Starling", "Germosen", "Admin");

            await _context.SaveChangesAsync();

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
                //CityId = context.Countries.FirstOrDefault().Cities.FirstOrDefault().Id,
                //City = context.Countries.FirstOrDefault().Cities.FirstOrDefault()
            };

            var result = await _userHelper.AddUserAsync(user, "123456");
            if (result != IdentityResult.Success)
            {
                throw new InvalidOperationException("Could not create the user in seeder");
            }

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
