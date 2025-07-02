using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.Models
{
    public enum ProjectStatus
    {
        Open, Assigned, InProgress, Submitted, Completed, Cancelled, Disputed
    }

    public class Project : BaseEntity
    {
        [Required]
        public Guid ClientId { get; set; } // FK to User - Client who posted
        public required User Client { get; set; } // Navigation property

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public decimal Budget { get; set; } // Fixed budget

        public DateTime? Deadline { get; set; }

        [Required]
        public ProjectStatus Status { get; set; } = ProjectStatus.Open;

        public Guid? AssignedFreelancerId { get; set; } // FK to User - Freelancer
        public User? AssignedFreelancer { get; set; } // Navigation property

        public DateTime? CompletionDate { get; set; }

        // Navigation properties
        public ICollection<Proposal>? Proposals { get; set; }
        public ICollection<File>? Attachments { get; set; } // Project attachments
        public ICollection<Rating>? Ratings { get; set; } // Ratings associated with this project

        // Navigation property for Skills (many-to-many)
        public ICollection<ProjectSkill>? ProjectSkills { get; set; }
    }
}