using System.ComponentModel.DataAnnotations;

public class MakeWithdrawalDto
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [Range(100, (double)decimal.MaxValue, ErrorMessage = "Withdrawal amount must be positive (greater than 100).")]
    public decimal Amount { get; set; }

    [StringLength(200)]
    public string Description { get; set; } = "ATM Withdrawal";
}