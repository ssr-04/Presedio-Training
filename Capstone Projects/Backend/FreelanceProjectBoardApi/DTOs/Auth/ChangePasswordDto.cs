using System.ComponentModel.DataAnnotations;
namespace FreelanceProjectBoardApi.DTOs.Auth
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        [Required, MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
        [Required, Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
