using System.ComponentModel.DataAnnotations;

public class MakeTransferDto
{
    [Required]
    public Guid SourceAccountId { get; set; }

    [Required]
    public Guid DestinationAccountId { get; set; }

    [Required]
    [Range(1, (double)decimal.MaxValue, ErrorMessage = "Transfer amount must be positive.")]
    public decimal Amount { get; set; }

    [StringLength(200)]
    public string Description { get; set; } = "Online Fund Transfer";
}