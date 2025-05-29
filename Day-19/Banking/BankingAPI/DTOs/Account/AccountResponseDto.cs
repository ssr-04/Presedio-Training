public class AccountResponseDto
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal Balance { get; set; }
    public DateTimeOffset OpeningDate { get; set; }
    public AccountStatus Status { get; set; }
    public bool IsActive { get; set; }
    public Guid CustomerId { get; set; }

    public string CustomerFirstName { get; set; } = string.Empty;
    public string CustomerLastName { get; set; } = string.Empty;
}