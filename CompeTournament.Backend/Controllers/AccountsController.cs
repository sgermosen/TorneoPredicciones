namespace CompeTournament.Backend.Controllers
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Data.Entities;
    using CompeTournament.Backend.Helpers;
    using CompeTournament.Backend.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using System.Linq;
    using System.Threading.Tasks;

    [AutoValidateAntiforgeryToken]
    public class AccountsController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        //private readonly ICountryRepository countryRepository;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AccountsController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            this._userHelper = userHelper;
            this._mailHelper = mailHelper;
            //   this.countryRepository = countryRepository;
            this._configuration = configuration;
            this._context = context;
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
                    var returnUrl = this.Request.Query["ReturnUrl"].FirstOrDefault();
                    if (this.Url.IsLocalUrl(returnUrl))
                    {
                        return this.LocalRedirect(returnUrl);
                    }

                    return this.RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    this.ModelState.AddModelError(string.Empty, "Tu cuenta quedó bloqueada por varios intentos fallidos. Intenta de nuevo en 15 minutos.");
                    return this.View(model);
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
                    var userType = await this._context.UserTypes.FirstOrDefaultAsync(ut => ut.Name == "Local")
                        ?? await this._context.UserTypes.FirstOrDefaultAsync();
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
                        foreach (var error in result.Errors)
                        {
                            this.ModelState.AddModelError(string.Empty, error.Description);
                        }

                        return this.View(model);
                    }

                    await this._userHelper.AddUserToRoleAsync(user, "User");

                    var myToken = await this._userHelper.GenerateEmailConfirmationTokenAsync(user);
                    var tokenLink = this.Url.Action("ConfirmEmail", "Accounts", new
                    {
                        userid = user.Id,
                        token = myToken
                    }, protocol: HttpContext.Request.Scheme);

                    var sent = this._mailHelper.SendMail(model.Username, "Copa Amistosa: confirma tu correo", $"<h1>Copa Amistosa</h1>" +
                                                                                                 $"Para confirmar tu correo, " +
                                                                                                 $"haz clic en este enlace:</br></br><a href = \"{tokenLink}\">Confirmar correo</a>");
                    this.ViewBag.Message = sent
                        ? "Tu cuenta quedó creada. Te enviamos un correo para confirmarla; ya puedes iniciar sesión."
                        : "Tu cuenta quedó creada. Ya puedes iniciar sesión.";
                    return this.View(model);
                }

                this.ModelState.AddModelError(string.Empty, "El usuario ya está registrado.");
            }

            return this.View(model);
        }

        [Authorize]
        public async Task<IActionResult> ChangeUser()
        {
            var user = await this._userHelper.GetUserByEmailAsync(this.User.Identity.Name);
            var model = new ChangeUserViewModel();
            this.ViewBag.UserMessage = this.TempData["UserMessage"];

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

        [Authorize]
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
                        this.ViewBag.UserMessage = "Tus datos quedaron guardados.";
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

        [Authorize]
        public IActionResult ChangePassword()
        {
            return this.View();
        }

        [Authorize]
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
                        this.TempData["UserMessage"] = "Tu contraseña quedó cambiada.";
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
                if (user != null)
                {
                    var myToken = await this._userHelper.GeneratePasswordResetTokenAsync(user);
                    var link = this.Url.Action("ResetPassword", "Accounts", new { token = myToken }, protocol: HttpContext.Request.Scheme);
                    this._mailHelper.SendMail(model.Email, "Copa Amistosa: recupera tu contraseña", $"<h1>Copa Amistosa</h1>" +
                                                                                                  $"Para cambiar tu contraseña, haz clic en este enlace:</br></br>" +
                                                                                                  $"<a href = \"{link}\">Cambiar contraseña</a>");
                }

                this.ViewBag.Message = "Si el correo está registrado, te enviamos las instrucciones para recuperar tu contraseña.";
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
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this._userHelper.GetUserByEmailAsync(model.UserName);
            if (user != null)
            {
                var result = await this._userHelper.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    this.ViewBag.Message = "Tu contraseña quedó cambiada. Ya puedes iniciar sesión.";
                    return this.View();
                }

                foreach (var error in result.Errors)
                {
                    this.ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            this.ModelState.AddModelError(string.Empty, "No pudimos cambiar la contraseña. Revisa el correo y vuelve a pedir el enlace.");
            return View(model);
        }

    }
}
