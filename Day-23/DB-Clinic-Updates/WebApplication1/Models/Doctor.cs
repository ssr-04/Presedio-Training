public class Doctor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // like "Active", "On Leave"
    public float YearsOfExperience { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false; // Soft delete flag

    // Navigation properties for many-to-many and one-to-many
    public ICollection<DoctorSpeciality>? DoctorSpecialities { get; set; }
    public ICollection<Appointment>? Appointments { get; set; }
    public User? User;
}