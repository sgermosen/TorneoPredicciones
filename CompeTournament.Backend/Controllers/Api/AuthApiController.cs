namespace CompeTournament.Backend.Controllers.Api
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Data.Entities;
    using CompeTournament.Backend.Helpers;
    using CompeTournament.Shared.Auth;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.RateLimiting;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("api/auth")]
    public class AuthApiController : ApiControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _context;

        public AuthApiController(IUserHelper userHelper, ITokenService tokenService, ApplicationDbContext context)
        {
            _userHelper = userHelper;
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("token")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<TokenResponse>> Token([FromBody] TokenRequest request)
        {
            var user = await _userHelper.GetUserByEmailAsync(request.Username);
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _userHelper.ValidatePasswordAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            var points = await GetUserPointsAsync(user.Id);
            var response = await _tokenService.CreateTokenAsync(user, points);
            return Ok(response);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request)
        {
            var existing = await _userHelper.GetUserByEmailAsync(request.Email);
            if (existing != null)
            {
                return Conflict(new { message = "El usuario ya esta registrado." });
            }

            var localType = await _context.UserTypes.FirstOrDefaultAsync(ut => ut.Name == "Local")
                ?? await _context.UserTypes.FirstOrDefaultAsync();

            var user = new ApplicationUser
            {
                Name = request.FirstName,
                Lastname = request.LastName,
                Email = request.Email,
                UserName = request.Email,
                PhoneNumber = request.PhoneNumber,
                UserType = localType
            };

            var result = await _userHelper.AddUserAsync(user, request.Password);
            if (result != IdentityResult.Success)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { errors });
            }

            await _userHelper.AddUserToRoleAsync(user, "User");
            var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
            await _userHelper.ConfirmEmailAsync(user, token);

            return Ok(new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Points = 0
            });
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> Me()
        {
            var user = await _userHelper.GetUserByIdAsync(CurrentUserId);
            if (user == null)
            {
                return NotFound();
            }

            var points = await GetUserPointsAsync(user.Id);
            return Ok(new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Points = points
            });
        }

        private async Task<int> GetUserPointsAsync(string userId)
        {
            return await _context.GroupUsers
                .Where(gu => gu.ApplicationUserId == userId && gu.IsAccepted && !gu.IsBlocked)
                .SumAsync(gu => (int?)gu.Points) ?? 0;
        }
    }
}
