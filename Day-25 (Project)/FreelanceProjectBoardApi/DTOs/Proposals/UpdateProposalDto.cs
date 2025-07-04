using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.DTOs.Proposals
{
    public class UpdateProposalDto
    {

        [Required, Range(0.01, 1000000.00)] 
        public decimal ProposedBudget { get; set; }
        
        [Required(ErrorMessage = "Deadline date and time is required.")]
        [RegularExpression(@"^\d{2}-\d{2}-\d{4} \d{2}:\d{2}$", ErrorMessage = "Deadline date time must be in 'dd-MM-yyyy hh:mm' format.")]
        public string ProposedDeadline { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string CoverLetter { get; set; } = string.Empty;

        // No status, will be handled by other endpoints
    }
}
