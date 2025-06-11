using System.ComponentModel.DataAnnotations;
using FreelanceProjectBoardApi.DTOs.Skills;

namespace FreelanceProjectBoardApi.DTOs.Projects
{
    public class UpdateProjectDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [Range(0.01, 1000000.00)]
        public decimal? Budget { get; set; }

        public List<CreateSkillDto>? Skills { get; set; }

        [RegularExpression(@"^\d{2}-\d{2}-\d{4} \d{2}:\d{2}$", ErrorMessage = "Deadline date time must be in 'dd-MM-yyyy hh:mm' format.")]
        public string Deadline { get; set; } = string.Empty;

        // Status updates will be handled via specific methods ( AssignFreelancer, CompleteProject)
        // or maybe a dedicated DTO for status changes
        // public ProjectStatus? Status { get; set; }

        public Guid? AssignedFreelancerId { get; set; } // To assign or re-assign freelancer
    }
}
