public class CustomerResponseDto
{
    public Guid CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTimeOffset DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateTimeOffset RegistrationDate { get; set; }
    public bool IsActive { get; set; }
        
    public ICollection<AccountResponseDto>? Accounts { get; set; }
}