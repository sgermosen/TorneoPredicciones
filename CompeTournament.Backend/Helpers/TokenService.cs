namespace CompeTournament.Backend.Helpers
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Data.Entities;
    using CompeTournament.Shared.Auth;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _context = context;
        }

        public async Task<TokenResponse> CreateTokenAsync(ApplicationUser user, int points)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var minutes = int.TryParse(_configuration["Tokens:AccessTokenMinutes"], out var configured) ? configured : 60;
            var expiration = DateTime.UtcNow.AddMinutes(minutes);

            var token = new JwtSecurityToken(
                _configuration["Tokens:Issuer"],
                _configuration["Tokens:Audience"],
                claims,
                expires: expiration,
                signingCredentials: credentials);

            var refreshToken = await CreateRefreshTokenAsync(user.Id);

            return new TokenResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.ExpiresAt,
                User = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Points = points
                }
            };
        }

        public async Task<TokenResponse> RefreshAsync(string refreshToken)
        {
            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (stored == null || !stored.IsActive)
            {
                return null;
            }

            stored.RevokedAt = DateTime.UtcNow;

            var user = await _userManager.FindByIdAsync(stored.ApplicationUserId);
            if (user == null)
            {
                await _context.SaveChangesAsync();
                return null;
            }

            var points = await _context.GroupUsers
                .Where(gu => gu.ApplicationUserId == user.Id && gu.IsAccepted && !gu.IsBlocked)
                .SumAsync(gu => (int?)gu.Points) ?? 0;

            return await CreateTokenAsync(user, points);
        }

        public async Task RevokeAsync(string refreshToken)
        {
            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (stored != null && stored.RevokedAt == null)
            {
                stored.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private async Task<RefreshToken> CreateRefreshTokenAsync(string userId)
        {
            var days = int.TryParse(_configuration["Tokens:RefreshTokenDays"], out var configured) ? configured : 30;

            var entity = new RefreshToken
            {
                Token = GenerateSecureToken(),
                ApplicationUserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(days)
            };

            _context.RefreshTokens.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        private static string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(48);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", string.Empty);
        }
    }
}
