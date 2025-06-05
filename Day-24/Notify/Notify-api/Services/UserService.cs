using AutoMapper;
using NotifyService.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<User> GetOrCreateUserAsync(string auth0UserId, string email, string? initialRole = null)
    {
        var user = await _userRepository.GetUserByAuth0UserIdAsync(auth0UserId);

        if (user == null)
        {
            // User does not exist in local DB, creating one
            user = new User
            {
                Auth0UserId = auth0UserId,
                Username = email,
                Role = initialRole ?? "Standard_User", // Default Standard_User if no initial role is specified
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow 
            };
            user = await _userRepository.AddUserAsync(user);
            Console.WriteLine($"New user provisioned: {email} with role {user.Role}");
        }
        else
        {
            await UpdateUserLastLoginAsync(auth0UserId);
        }
        return user;
    }

    public async Task<UserResponseDto?> GetUserByAuth0UserIdAsync(string auth0UserId)
    {
        var user = await _userRepository.GetUserByAuth0UserIdAsync(auth0UserId);
        return user == null ? null : _mapper.Map<UserResponseDto>(user);
    }

    public async Task<bool> UpdateUserLastLoginAsync(string auth0UserId)
    {
        var user = await _userRepository.GetUserByAuth0UserIdAsync(auth0UserId);
        if (user == null)
        {
            return false;
        }
        user.LastLogin = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user);
        return true;
    }

    public async Task<bool> UserExistsByAuth0UserIdAsync(string auth0UserId)
    {
        return await _userRepository.UserExistsByAuth0UserIdAsync(auth0UserId);
    }
}