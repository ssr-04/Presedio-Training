using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UserRepository : IUserRepository
{
    private readonly ClinicContext _context;

    public UserRepository(ClinicContext context)
    {
        _context = context;
    }

    public async Task<User> AddUserAsync(User user)
    {

        if (user.CreatedAt == default)
        {
            user.CreatedAt = DateTime.UtcNow;
        }
        user.IsActive = true;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
                             .Include(u => u.Patient)
                             .Include(u => u.Doctor)
                             .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
                             .Include(u => u.Patient)
                             .Include(u => u.Doctor)
                             .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<User?> UpdateUserAsync(User user)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

        if (existingUser == null)
        {
            return null; // User not found
        }

        existingUser.IsActive = user.IsActive;
        existingUser.LastLogin = user.LastLogin;
        _context.Users.Update(existingUser);
        await _context.SaveChangesAsync();
        return existingUser;
    }

    public async Task<bool> UserExistsAsync(int id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }

    public async Task<bool> UserExistsByUsernameAsync(string username)
    {
        // Case-insensitive comparison
        return await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }

}