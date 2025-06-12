using FreelanceProjectBoardApi.DTOs.Skills;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.DTOs.Projects
{
    public class ProjectListDto
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public string ClientCompanyName { get; set; } = string.Empty; 
        public string Title { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public List<SkillDto>? SkillsRequired { get; set; }
        public DateTime? Deadline { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
