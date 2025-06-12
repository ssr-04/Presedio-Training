using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.DTOs.ClientProfiles
{
    public class CreateClientProfileDto
    {
        // UserId will be derived from authenticated user
        [Required, MaxLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(256)]
        public string? ContactPersonName { get; set; }
    }
}
