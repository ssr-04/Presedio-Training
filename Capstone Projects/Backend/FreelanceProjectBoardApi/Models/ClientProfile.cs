using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.Models
{
    public class ClientProfile : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; } //FK to User
        public required User User { get; set; }  // Navigation Property

        [MaxLength(75)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Location { get; set; } // City, Country

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(256)]
        public string? ContactPersonName { get; set; } // Optional (as it's default to User)

        // Navigation Property to Projects posted by this client
        public ICollection<Project>? PostedProjects { get; set; }
    }
}