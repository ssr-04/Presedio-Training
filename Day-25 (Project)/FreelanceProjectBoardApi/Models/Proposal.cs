using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.Models
{
    public enum ProposalStatus
    {
        Pending,
        Accepted,
        Rejected,
        Withdrawn
    }

    public class Proposal : BaseEntity
    {
        [Required]
        public Guid ProjectId { get; set; } // FK - Project
        public required Project Project { get; set; } // Navigation property

        [Required]
        public Guid FreelancerId { get; set; } // FK - freelancer
        public required User Freelancer { get; set; } // Navigation Property

        public decimal ProposedBudget { get; set; }

        public DateTime? ProposedDeadLine { get; set; }

        [Required]
        [MaxLength(2000)]
        public string CoverLetter { get; set; } = string.Empty;

        [Required]
        public ProposalStatus Status { get; set; } = ProposalStatus.Pending;

        // Navigation property to any attachments for this proposal
        public ICollection<File>? Attachments { get; set; }
    }
}