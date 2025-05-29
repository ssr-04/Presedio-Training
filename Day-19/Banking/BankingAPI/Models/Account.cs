
public class Account
{
    public Guid AccountId { get; set; } // Primary Key
    public string AccountNumber { get; set; } = string.Empty; 
    public AccountType AccountType { get; set; }
    public decimal Balance { get; set; }
    public DateTimeOffset OpeningDate { get; set; }
    public AccountStatus Status { get; set; }
    public bool IsActive { get; set; } // For soft delete

    // Foreign Key to Customer
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; } // Navigation property

    // Navigation property
    public ICollection<Transaction>? Transactions { get; set; }
}
