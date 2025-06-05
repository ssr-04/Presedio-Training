using System.Threading.Tasks;

namespace NotifyService.Services
{
    public interface IUserService
    {
        Task<User> GetOrCreateUserAsync(string auth0UserId, string email, string? initialRole = null);
        Task<UserResponseDto?> GetUserByAuth0UserIdAsync(string auth0UserId);
        Task<bool> UpdateUserLastLoginAsync(string auth0UserId);
        Task<bool> UserExistsByAuth0UserIdAsync(string auth0UserId);
    }
}