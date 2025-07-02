using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.Models
{
    public class FreelancerSkill : BaseEntity 
    {
        // Joining Table Freelancer + skills
        [Required]
        public Guid FreelancerProfileId { get; set; }
        public required FreelancerProfile FreelancerProfile { get; set; } // Navigation property

        [Required]
        public Guid SkillId { get; set; }
        public required Skill Skill { get; set; } // Navigation property
    }
}
