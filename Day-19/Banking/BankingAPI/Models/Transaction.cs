public class Transaction
{
    public Guid TransactionId { get; set; } // Primary Key (different for sender and receiver)
    public Guid AccountId { get; set; } // Foreign Key to account 
    public decimal Amount { get; set; } 
    public bool IsDebit { get; set; } 
    public TransactionType TransactionType { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty; // same for both receiver and sender
    public TransactionStatus Status { get; set; }
    public decimal BalanceAfterTransaction { get; set; }

    public Account? Account { get; set; } // Navigation property
}
