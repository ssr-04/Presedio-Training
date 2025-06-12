using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.DTOs.Users
{
    public class UpdateUserDto
    {
        [EmailAddress]
        public string? Email { get; set; }
    }
}
