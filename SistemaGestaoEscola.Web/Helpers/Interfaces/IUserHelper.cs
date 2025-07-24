using Microsoft.AspNetCore.Identity;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Models;

namespace SistemaGestaoEscola.Web.Helpers.Interfaces
{
    public interface IUserHelper
    {
        Task<User> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task<SignInResult> LoginAsync(LoginViewModel model);

        Task LogOutAsync();

        Task<IdentityResult> UpdateUserAsync(User user);

        Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword);

        Task CheckRoleAsync(string roleName);

        Task AddUserToRoleAsync(User user, string roleName);

        Task<bool> IsUserInRoleAsync(User user, string roleName);

        Task<string> GenerateEmailConfirmationTokenAsync(User user);

        Task<string> GeneratePasswordResetTokenAsync(User user);

        Task<IdentityResult> ConfirmEmailAsync(User user, string token);

        Task<IList<string>> GetRolesAsync(User user);

        Task<IdentityResult> ResetPasswordAsync(User user, string token, string password);

        Task<IEnumerable<User>> GetAllUsersAsync();

        Task<User> GetUserByIdAsync(string userId);

        Task<IdentityResult> DeleteUserAsync(User user);

        Task<IdentityResult> RemoveFromRoleAsync(User user, string role);

        Task<IEnumerable<User>> GetAllUsersByRoleAsync(string role);
    }
}
