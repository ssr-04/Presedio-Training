using FreelanceProjectBoardApi.DTOs.Files;
using FreelanceProjectBoardApi.DTOs.Skills;

namespace FreelanceProjectBoardApi.DTOs.FreelancerProfiles
{
    public class FreelancerProfileResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Headline { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public List<SkillDto>? Skills { get; set; } // List of Skill DTOs
        public string? ExperienceLevel { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? PortfolioUrl { get; set; }
        public FileResponseDto? ResumeFile { get; set; }
        public FileResponseDto? ProfilePictureFile { get; set; }
        public bool IsAvailable { get; set; }
        public int ProjectsCompleted { get; set; }
        public double AverageRating { get; set; } // Will calculate
        public DateTime CreatedAt { get; set; }
    }
}
