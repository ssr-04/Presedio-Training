using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(256)]
    public string Username { get; set; } = string.Empty; // Email address

    [Required]
    public byte[] PasswordHash { get; set; } = new byte[0]; // Stores the hashed password

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