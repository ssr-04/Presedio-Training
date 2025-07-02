using FreelanceProjectBoardApi.DTOs.Files;
using FreelanceProjectBoardApi.DTOs.Projects;
using FreelanceProjectBoardApi.DTOs.Users;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.DTOs.Proposals
{
    public class ProposalResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid FreelancerId { get; set; }
        public ProjectListDto? Project { get; set; } // Simpler project view
        public UserResponseDto? Freelancer { get; set; } // Basic freelancer user info
        public decimal ProposedBudget { get; set; }
        public string ProposedDeadline { get; set; } = string.Empty;
        public string CoverLetter { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<FileResponseDto>? Attachments { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
