using System.ComponentModel.DataAnnotations;
using FreelanceProjectBoardApi.DTOs.Skills;

namespace FreelanceProjectBoardApi.DTOs.Projects
{
    public class CreateProjectDto
    {
        // ClientId will be derived from authenticated user
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required, Range(0.01, 1000000.00)] // Example range
        public decimal Budget { get; set; }

        public List<CreateSkillDto>? Skills { get; set; }

        [Required(ErrorMessage = "Deadline date and time is required.")]
        [RegularExpression(@"^\d{2}-\d{2}-\d{4} \d{2}:\d{2}$", ErrorMessage = "Deadline date time must be in 'dd-MM-yyyy hh:mm' format.")]
        public string Deadline { get; set; } = string.Empty;

    }
}
