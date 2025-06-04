using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(256)]
    public string Username { get; set; } = string.Empty; // Email address

    // Auth0 provides a unique user ID. It's best to store this to link your local user to Auth0.
    // It's typically a string like "auth0|1234567890abcdef"
    [StringLength(256)]
    public string? Auth0UserId { get; set; } // New property to store Auth0's user ID


    [Required]
    [StringLength(50)]
    public string Role { get; set; } = string.Empty; // "Admin", "Patient", "Doctor", "Staff"


    public bool IsActive { get; set; } = true; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }

    public int? PatientId { get; set; }
    public Patient? Patient { get; set; }

    public int? DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

}