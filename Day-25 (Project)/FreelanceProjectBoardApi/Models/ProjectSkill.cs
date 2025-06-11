using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.Models
{
    public class ProjectSkill : BaseEntity
    {
        // Joining table Project + Skills
        [Required]
        public Guid ProjectId { get; set; }
        public required Project Project { get; set; } // Navigation Property

        [Required]
        public Guid SkillId { get; set; }
        public required Skill Skill { get; set; } // Navigation Property
    }
}
