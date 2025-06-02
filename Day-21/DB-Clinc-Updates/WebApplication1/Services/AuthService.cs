using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration; 

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        // user from the database by username (email)
        var user = await _userRepository.GetUserByUsernameAsync(username);

        // if user exists and is active
        if (user == null || !user.IsActive)
        {
            return null; 
        }

        // provided password against the stored hash
        bool isPasswordValid = _passwordHasher.VerifyPassword(password, user.PasswordHash);

        if (!isPasswordValid)
        {
            return null; // Invalid credentials
        }

        // Updating LastLogin timestamp
        user.LastLogin = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user);

        // Generating JWT token
        return GenerateJwtToken(user);
    }

    public List<Claim> GetUserClaims(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username), // Subject (unique - email)
            new Claim(ClaimTypes.Role, user.Role) // User's role for authorization
        };

        // Adding PatientId or DoctorId 
        if (user.PatientId.HasValue)
        {
            claims.Add(new Claim("patient_id", user.PatientId.Value.ToString()));
        }
        if (user.DoctorId.HasValue)
        {
            claims.Add(new Claim("doctor_id", user.DoctorId.Value.ToString()));
        }

        return claims;
    }


    private string GenerateJwtToken(User user)
    {
        var claims = GetUserClaims(user); 

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!); //secret key
        
        var expiryMinutes = double.Parse(jwtSettings["ExpiryMinutes"]!);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}