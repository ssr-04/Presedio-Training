using System.ComponentModel.DataAnnotations;

public class CreateAccountDto
{
    [Required]
    public Guid CustomerId { get; set; } 

    [Required]
    [EnumDataType(typeof(AccountType))] //ensures it's a enum type
    public string AccountType { get; set; } = string.Empty; 

    [Range(0.00, (double)decimal.MaxValue, ErrorMessage = "Initial deposit cannot be negative.")]
    public decimal InitialDeposit { get; set; } = 0m; // defaults to 0
}