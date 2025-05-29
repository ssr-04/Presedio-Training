public class Customer
{
    public Guid CustomerId { get; set; } // Primary Key
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTimeOffset DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty; // complete address as string for simplicity
    public string ContactNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty; 
    public DateTimeOffset RegistrationDate { get; set; }
    public bool IsActive { get; set; } // For soft delete

    // Navigation property 
    public ICollection<Account>? Accounts { get; set; }
}