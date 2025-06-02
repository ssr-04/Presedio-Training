using AutoMapper;
using Microsoft.Extensions.Configuration; // For JWT settings
using Microsoft.IdentityModel.Tokens; // For JWT
using System.IdentityModel.Tokens.Jwt; // For JWT
using System.Security.Claims; // For JWT claims
using System.Text;
using BCrypt.Net;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public AuthService(IUserRepository userRepository, IConfiguration configuration, IMapper mapper)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request)
    {
        // 1. Check if username or email already exists
        if (await _userRepository.GetByUsernameAsync(request.Username) != null)
        {
            // In a real application, you might throw a custom exception or return a specific error code
            throw new ApplicationException("Username already taken.");
        }
        // Add email check if email is supposed to be unique and required
        //if (request.Email != null && await _userRepository.GetByEmailAsync(request.Email) != null) { ... }


        // 2. Hash password (using BCrypt for example)
        // Install: Install-Package BCrypt.Net-Next
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 3. Create User model
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email ?? string.Empty,
            PasswordHash = passwordHash,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // 4. Save to database
        await _userRepository.AddAsync(user);
        if (!await _userRepository.SaveChangesAsync())
        {
            throw new ApplicationException("Failed to register user. Database error.");
        }

        // 5. Generate JWT token
        var token = GenerateJwtToken(user);

        // 6. Return response DTO
        return new AuthResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            Token = token
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
    {
        // 1. Retrieve user by username
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            // User not found, invalid credentials
            return null;
        }

        // 2. Verify password hash
        // Install: Install-Package BCrypt.Net-Next
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // Incorrect password
            return null;
        }

        // 3. Update LastLoginAt
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync(); // Save the update

        // 4. Generate JWT token
        var token = GenerateJwtToken(user);

        // 5. Return response DTO
        return new AuthResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            Token = token
        };
    }

    private string GenerateJwtToken(User user)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
            // Add more claims as needed, e.g., roles
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:DurationInMinutes"] ?? "60")),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured."))),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        System.Console.WriteLine(tokenHandler.WriteToken(token));
        return tokenHandler.WriteToken(token);
    }
}