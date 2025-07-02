using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;

namespace FreelanceProjectBoardApi.Models
{
    public enum UserType
    {
        Client,
        Freelancer,
        Admin

        // Maybe both?
    }

    public class User : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [MaxLength(75)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(265)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserType Type { get; set; } = UserType.Freelancer;

        public string? RefreshToken { get; set; } // Stores the refresh token string
        public DateTime RefreshTokenExpiryTime { get; set; } // Stores the expiry time of the refresh token

        // Navigation Properties
        public ClientProfile? ClientProfile { get; set; }
        public FreelancerProfile? FreelancerProfile { get; set; }

        // Navigation properties for stuff related to this user
        public ICollection<Project>? PostedProjects { get; set; }  // Projects posted by this user (if Client)
        public ICollection<Project>? AssignedProjects { get; set; } // Projects assinged to this user (if Freelancer)

        public ICollection<Proposal>? Proposals { get; set; } // Proposals submitted by this user (if Freelancer)
        public ICollection<Rating>? RatingsGiven { get; set; } // Ratings given by this user
        public ICollection<Rating>? RatingsReceived { get; set; } // Ratings received by this user
        public ICollection<File>? UploadedFiles { get; set; } // Files uploaded by this user

    }
}