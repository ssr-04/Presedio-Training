public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task AddAsync(User user);
    Task UpdateAsync(User user); // For updating user details (e.g., email, password hash)
    Task DeleteAsync(User user); // Or DeleteByIdAsync(Guid id)
    Task<bool> SaveChangesAsync(); // For Unit of Work pattern
}