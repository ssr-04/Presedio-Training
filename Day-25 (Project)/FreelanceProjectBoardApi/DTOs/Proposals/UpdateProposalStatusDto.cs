using System.ComponentModel.DataAnnotations;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.DTOs.Proposals
{
    public class UpdateProposalStatusDto
    {
        [Required]
        public string NewStatus { get; set; } = string.Empty;
    }
}
