public class TransactionResponseDto
{
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public bool IsDebit { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public TransactionStatus Status { get; set; }
    public decimal BalanceAfterTransaction { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
}