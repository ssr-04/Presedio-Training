using System.ComponentModel.DataAnnotations;

public class UpdateCustomerDto
{
    [StringLength(20, MinimumLength = 3)]
    public string? FirstName { get; set; }

    [StringLength(20, MinimumLength = 3)]
    public string? LastName { get; set; }

    [DataType(DataType.Date)]
    public DateTimeOffset? DateOfBirth { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [Phone]
    [StringLength(20)]
    public string? ContactNumber { get; set; }

    [EmailAddress]
    [StringLength(75)]
    public string? Email { get; set; }
}