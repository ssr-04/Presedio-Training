using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using FreelanceProjectBoardApi.DTOs.Auth;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace FreelanceProjectBoardApi.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IClientProfileRepository _clientProfileRepository;
        private readonly IFreelancerProfileRepository _freelancerProfileRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IClientProfileRepository clientProfileRepository,
            IFreelancerProfileRepository freelancerProfileRepository,
            IMapper mapper,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _clientProfileRepository = clientProfileRepository;
            _freelancerProfileRepository = freelancerProfileRepository;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> LoginAsync(UserLoginDto loginDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(loginDto.Email, includeProfiles: false);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }

            // tokens upon successful login
            return await GenerateAuthTokens(user);
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetAllAsync(
                filter: u => u.RefreshToken == refreshToken && u.RefreshTokenExpiryTime > DateTime.UtcNow,
                includeDeleted: false
            );
            var foundUser = user.FirstOrDefault();

            if (foundUser == null)
            {
                return null; // Invalid or expired refresh token
            }

            // Generate new tokens
            return await GenerateAuthTokens(foundUser);
        }

        public async Task<AuthResponseDto?> RegisterAsync(UserRegisterDto registerDto)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(registerDto.Email, includeProfiles: false);
            if (existingUser != null)
            {
                return null;
            }

            var newUser = new User
            {
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Type = Enum.TryParse<FreelanceProjectBoardApi.Models.UserType>(registerDto.UserType, out var parsedUserType)
                        ? parsedUserType
                        : throw new ArgumentException("Invalid user type"),
                IsDeleted = false
            };

            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();

            // if (newUser.Type == UserType.Client)
            // {
            //     var clientProfile = new ClientProfile { UserId = newUser.Id, User = newUser };
            //     await _clientProfileRepository.AddAsync(clientProfile);
            // }
            // if (newUser.Type == UserType.Freelancer)
            // {
            //     var freelancerProfile = new FreelancerProfile { UserId = newUser.Id, User = newUser };
            //     await _freelancerProfileRepository.AddAsync(freelancerProfile);
            // }

            // await _userRepository.SaveChangesAsync();

            return await GenerateAuthTokens(newUser);
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetAllAsync(
                filter: u => u.RefreshToken == refreshToken,
                includeDeleted: false
            );
            var foundUser = user.FirstOrDefault();

            if (foundUser == null)
            {
                return false; // Token not found or already revoked
            }

            foundUser.RefreshToken = null;
            foundUser.RefreshTokenExpiryTime = DateTime.MinValue; // Setting to min value to clearly mark as invalid

            await _userRepository.UpdateAsync(foundUser);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        private async Task<AuthResponseDto> GenerateAuthTokens(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Type.ToString())
            };

            var jwtSecret = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT Key is not configured");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"] ?? "60")),
                SigningCredentials = credentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            // Generating and save refresh token
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7")); // Refresh token expiry

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
                Token = accessToken,
                RefreshToken = refreshToken,
                UserType = user.Type.ToString()
            };
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}