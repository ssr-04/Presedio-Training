public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
    // Logout is typically client-side for stateless JWTs, no server-side method often needed here.
    // If refresh tokens were implemented, a method to revoke them would be here.
}
