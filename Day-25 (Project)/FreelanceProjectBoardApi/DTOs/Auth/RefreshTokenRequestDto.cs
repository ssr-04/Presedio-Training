using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.DTOs.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
