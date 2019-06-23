namespace CompeTournament.Backend.Controllers
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Data.Entities;
    using CompeTournament.Backend.Helpers;
    using CompeTournament.Backend.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    public class AccountsController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        //private readonly ICountryRepository countryRepository;
        private readonly IConfiguration _configuration;

        public AccountsController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IConfiguration configuration)
        {
            this._userHelper = userHelper;
            this._mailHelper = mailHelper;
            //   this.countryRepository = countryRepository;
            this._configuration = configuration;
        }

        public IActionResult Login()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                return this.RedirectToAction("Index", "Home");
            }

            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var result = await this._userHelper.LoginAsync(model);
                if (result.Succeeded)
                {
                    if (this.Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        return this.Redirect(this.Request.Query["ReturnUrl"].First());
                    }

                    return this.RedirectToAction("Index", "Home");
                }
            }

            this.ModelState.AddModelError(string.Empty, "Acceso Fallido.");
            return this.View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await this._userHelper.LogoutAsync();
            return this.RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            //var model = new RegisterNewUserViewModel
            //{
            //    Countries = this.countryRepository.GetComboCountries(),
            //    Cities = this.countryRepository.GetComboCities(0)
            //};

            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterNewUserViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this._userHelper.GetUserByEmailAsync(model.Username);
                if (user == null)
                {
                    // var userType = await this.countryRepository.GetCityAsync(model.CityId);
                    var userType = new UserType { Id = 1, Name = "Local" };
                    user = new ApplicationUser
                    {
                        Name = model.FirstName,
                        Lastname = model.LastName,
                        Email = model.Username,
                        UserName = model.Username,
                        PhoneNumber = model.PhoneNumber,
                        UserType = userType
                        //CityId = model.CityId,
                        //City = city
                    };

                    var result = await this._userHelper.AddUserAsync(user, model.Password);
                    if (result != IdentityResult.Success)
                    {
                        this.ModelState.AddModelError(string.Empty, "Usuario no pudo ser creado");
                        return this.View(model);
                    }

                    var myToken = await this._userHelper.GenerateEmailConfirmationTokenAsync(user);
                    var tokenLink = this.Url.Action("ConfirmEmail", "Account", new
                    {
                        userid = user.Id,
                        token = myToken
                    }, protocol: HttpContext.Request.Scheme);

                    this._mailHelper.SendMail(model.Username, "911 Reactions Email confirmation", $"<h1>911 Reactions Email confirmation</h1>" +
                                                                                                 $"Para permitir el acceso a tu usuario, " +
                                                                                                 $"porfavor, has click en este link:</br></br><a href = \"{tokenLink}\">Confirmar Correo</a>");
                    this.ViewBag.Message = "Las instrucciones para permitir el acceso a tu usuario, han sido enviadas a tu correo.";
                    return this.View(model);
                }

                this.ModelState.AddModelError(string.Empty, "El usuario ya esta registrado.");
            }

            return this.View(model);
        }

        public async Task<IActionResult> ChangeUser()
        {
            var user = await this._userHelper.GetUserByEmailAsync(this.User.Identity.Name);
            var model = new ChangeUserViewModel();

            if (user != null)
            {
                model.FirstName = user.Name;
                model.LastName = user.Lastname;
                model.PhoneNumber = user.PhoneNumber;

                //var city = await this.countryRepository.GetCityAsync(user.CityId);
                //if (city != null)
                //{
                //    var country = await this.countryRepository.GetCountryAsync(city);
                //    if (country != null)
                //    {
                //        model.CountryId = country.Id;
                //        model.Cities = this.countryRepository.GetComboCities(country.Id);
                //        model.Countries = this.countryRepository.GetComboCountries();
                //        model.CityId = user.CityId;
                //    }
                //}
            }

            //model.Cities = this.countryRepository.GetComboCities(model.CountryId);
            //model.Countries = this.countryRepository.GetComboCountries();
            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUser(ChangeUserViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this._userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                if (user != null)
                {
                    //  var city = await this.countryRepository.GetCityAsync(model.CityId);

                    user.Name = model.FirstName;
                    user.Lastname = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    //user.CityId = model.CityId;
                    //user.City = city;

                    var respose = await this._userHelper.UpdateUserAsync(user);
                    if (respose.Succeeded)
                    {
                        this.ViewBag.UserMessage = "Usuario Actualizado!";
                    }
                    else
                    {
                        this.ModelState.AddModelError(string.Empty, respose.Errors.FirstOrDefault().Description);
                    }
                }
                else
                {
                    this.ModelState.AddModelError(string.Empty, "Usuario no Encontrado.");
                }
            }

            return this.View(model);
        }

        public IActionResult ChangePassword()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this._userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                if (user != null)
                {
                    var result = await this._userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return this.RedirectToAction("ChangeUser");
                    }
                    else
                    {
                        this.ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault().Description);
                    }
                }
                else
                {
                    this.ModelState.AddModelError(string.Empty, "Usuario no Encontrado!!.");
                }
            }

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this._userHelper.GetUserByEmailAsync(model.Username);
                if (user != null)
                {
                    var result = await this._userHelper.ValidatePasswordAsync(
                        user,
                        model.Password);

                    if (result.Succeeded)
                    {
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._configuration["Tokens:Key"]));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var token = new JwtSecurityToken(
                            this._configuration["Tokens:Issuer"],
                            this._configuration["Tokens:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddDays(15),
                            signingCredentials: credentials);
                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        };

                        return this.Created(string.Empty, results);
                    }
                }
            }

            return this.BadRequest();
        }

        public IActionResult NotAuthorized()
        {
            return this.View();
        }

        //public async Task<JsonResult> GetCitiesAsync(int countryId)
        //{
        //   // var country = await this.countryRepository.GetCountryWithCitiesAsync(countryId);
        //    //return this.Json(country.Cities.OrderBy(c => c.Name));
        //}

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return this.NotFound();
            }

            var user = await this._userHelper.GetUserByIdAsync(userId);
            if (user == null)
            {
                return this.NotFound();
            }

            var result = await this._userHelper.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return this.NotFound();
            }

            return View();
        }

        public IActionResult RecoverPassword()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this._userHelper.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Este email no corresponde al usuario registrado.");
                    return this.View(model);
                }

                var myToken = await this._userHelper.GeneratePasswordResetTokenAsync(user);
                var link = this.Url.Action("ResetPassword", "Account", new { token = myToken }, protocol: HttpContext.Request.Scheme);
                var mailSender = new MailHelper(_configuration);
                mailSender.SendMail(model.Email, "Reacciones 911 Recuperacion de Contraseña", $"<h1>Reacciones 911 Recuperacion de Contraseña</h1>" +
                                                                                              $"Para resetear su contraseña, haga click en este link:</br></br>" +
                                                                                              $"<a href = \"{link}\">Resetear Contraseña</a>");
                this.ViewBag.Message = "Las instrucciones para recuperar tu contraseña, han sido enviadas a tu Correo";
                return this.View();

            }

            return this.View(model);
        }

        public IActionResult ResetPassword(string token)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await this._userHelper.GetUserByEmailAsync(model.UserName);
            if (user != null)
            {
                var result = await this._userHelper.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    this.ViewBag.Message = "Contraseña Reseteada Satisfactoriamente.";
                    return this.View();
                }

                this.ViewBag.Message = "Error mientras se intentaba resetear la contraseña.";
                return View(model);
            }

            this.ViewBag.Message = "Ususario no encontrado.";
            return View(model);
        }

    }
}
