using System.ComponentModel.DataAnnotations;
using FreelanceProjectBoardApi.DTOs.Skills;

namespace FreelanceProjectBoardApi.DTOs.FreelancerProfiles
{
    public class UpdateFreelancerProfileDto
    {
        [MaxLength(100)]
        public string? Headline { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

         public List<CreateSkillDto>? Skills { get; set; }
        [MaxLength(50)]
        public string? ExperienceLevel { get; set; }

        public decimal? HourlyRate { get; set; }

        [MaxLength(200)]
        public string? PortfolioUrl { get; set; }

        public bool? IsAvailable { get; set; } // Nullable for partial updates

        public int ProjectsCompleted { get; set; }
    }
}
