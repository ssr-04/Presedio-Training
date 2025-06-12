using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
    }
}