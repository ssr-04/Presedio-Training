using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.DTOs.ClientProfiles
{
    public class UpdateClientProfileDto
    {
        [MaxLength(100)]
        public string? CompanyName { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(256)]
        public string? ContactPersonName { get; set; }
    }
}
