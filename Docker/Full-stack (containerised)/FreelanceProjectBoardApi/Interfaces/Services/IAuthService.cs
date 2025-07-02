using FreelanceProjectBoardApi.DTOs.Auth;

namespace FreelanceProjectBoardApi.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(UserRegisterDto registerDto);
        Task<AuthResponseDto?> LoginAsync(UserLoginDto loginDto);
        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken); 
        Task<bool> RevokeTokenAsync(string refreshToken); 

        // Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto); // Might be in UserService
        // Task<bool> ResetPasswordAsync(string email); // For later, if implementing password reset
    }
}