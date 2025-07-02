using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.Models
{
    public class FreelancerProfile : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; } // FK to User
        public required User User { get; set; }  // Navigation Property

        [MaxLength(100)]
        public string Headline { get; set; } = string.Empty; // Something like Linkedin Headline

        [MaxLength(1000)]
        public string? Bio { get; set; }

        [MaxLength(50)]
        public string? ExperienceLevel { get; set; }

        public decimal? HourlyRate { get; set; } // Optional

        [MaxLength(200)]
        public string? PortfolioUrl { get; set; }

        public Guid? ResumeFileId { get; set; } // FK to File for Resume file
        public File? ResumeFile { get; set; } // Navigation property

        public Guid? ProfilePictureFileId { get; set; } // FK to File profile picture
        public File? ProfilePictureFile { get; set; } // Navigation property

        public bool IsAvailable { get; set; } = true; 

        public int ProjectsCompleted { get; set; } = 0;


        // Navigation properties for proposals and ratings
        public ICollection<Proposal>? SubmittedProposals { get; set; }
        public ICollection<Rating>? RatingsAsRatee { get; set; } // Ratings received by this freelancer

        // Navigation property for Skills (many-to-many)
        public ICollection<FreelancerSkill>? FreelancerSkills { get; set; } 
    }
}