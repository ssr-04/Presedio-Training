using System.ComponentModel.DataAnnotations;

public class CreateCustomerDto
{
    [Required]
    [StringLength(15, MinimumLength = 3)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(15, MinimumLength = 3)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTimeOffset DateOfBirth { get; set; }

    [StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string ContactNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(75)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(30, MinimumLength = 5)]
    public string NationalId { get; set; } = string.Empty;
}