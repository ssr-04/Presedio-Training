using System.Threading.Tasks;

public interface IUserRepository
{
    Task<User> AddUserAsync(User user);

    Task<User?> GetUserByIdAsync(int id);

    Task<User?> GetUserByUsernameAsync(string username);

    // Updating an existing user's details (e.g., IsActive, LastLogin, Role)
    Task<User?> UpdateUserAsync(User user);

    // Checking if a user exists by ID
    Task<bool> UserExistsAsync(int id);

    Task<bool> UserExistsByUsernameAsync(string username);

}