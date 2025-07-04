using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.Models
{
    public enum NotificationCategory
    {
        NewProposal,        // For clients when a freelancer submits a proposal
        ProposalAccepted,   // For freelancers when their proposal is accepted
        ProposalRejected,   // For freelancers when their proposal is rejected
        ProjectCompleted,
        NewRating,          // For the user who received a rating
        General             // For system announcements or other messages
    }
    
    public class Notification : BaseEntity
    {
        [Required]
        public Guid ReceiverId { get; set; } // FK to the User who should receive this
        public required User Receiver { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationCategory Category { get; set; }

        public bool IsRead { get; set; } = false;
    }
}