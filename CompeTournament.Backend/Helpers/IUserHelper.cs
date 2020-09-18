namespace CompeTournament.Backend.Helpers
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Models;
    using Microsoft.AspNetCore.Identity;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public interface IUserHelper
    {
        string RemoveCharacters(string param);

        Task<ApplicationUser> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(ApplicationUser user, string password);

        Task<SignInResult> LoginAsync(LoginViewModel model);

        Task LogoutAsync();

        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);

        Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string oldPassword, string newPassword);

        Task<SignInResult> ValidatePasswordAsync(ApplicationUser user, string password);

        Task CheckRoleAsync(string roleName);

        Task AddUserToRoleAsync(ApplicationUser user, string roleName);

        Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName);

        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);

        Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);

        Task<ApplicationUser> GetUserByIdAsync(string userId);

        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);

        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string password);

        Task<ApplicationUser> AddClaim(ApplicationUser user, Claim claim);

        Task<List<ApplicationUser>> GetAllUsersAsync();

        Task RemoveUserFromRoleAsync(ApplicationUser user, string roleName);

        Task DeleteUserAsync(ApplicationUser user);

    }
}
