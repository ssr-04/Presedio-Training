using FirstTwitterApp.Models;

namespace FirstTwitterApp.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync(bool includeDeleted = false);
        Task<User?> GetUserByIdAsync(int id, bool includeDeleted = false);
        Task<User?> GetUserByUsernameAsync(string username, bool includeDeleted = false);
        Task<User?> GetUserByEmailAsync(string email, bool includeDeleted = false);
        Task<User> AddUserAsync(User user);
        Task<User?> UpdateUserAsync(User user);
        Task<bool> SoftDeleteUserAsync(int id);
        Task<bool> UserExistsAsync(int id, bool includeDeleted = false);
        Task<bool> UsernameExistsAsync(string username, bool includeDeleted = false);
        Task<bool> EmailExistsAsync(string email, bool includeDeleted = false);
    }
}