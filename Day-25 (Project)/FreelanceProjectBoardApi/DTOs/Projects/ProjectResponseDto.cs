using FreelanceProjectBoardApi.DTOs.Files;
using FreelanceProjectBoardApi.DTOs.Proposals;
using FreelanceProjectBoardApi.DTOs.Ratings;
using FreelanceProjectBoardApi.DTOs.Skills;
using FreelanceProjectBoardApi.DTOs.Users;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.DTOs.Projects
{
    public class ProjectResponseDto
    {
         public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public UserResponseDto? Client { get; set; } // Basic client info
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public List<SkillDto>? SkillsRequired { get; set; }
        public DateTime? Deadline { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? AssignedFreelancerId { get; set; }
        public UserResponseDto? AssignedFreelancer { get; set; } // Basic freelancer info
        public DateTime? CompletionDate { get; set; }
        public List<FileResponseDto>? Attachments { get; set; }
        public List<RatingResponseDto>? Ratings { get; set; }
        public List<ProposalResponseDto>? Proposals { get; set; } // Limited details for proposals
        public DateTime CreatedAt { get; set; }
    }
}