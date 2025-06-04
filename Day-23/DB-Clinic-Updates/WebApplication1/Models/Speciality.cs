public class Speciality
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; 
    public bool IsDeleted { get; set; } = false; // Soft delete flag

    // Navigation property for many-to-many
    public ICollection<DoctorSpeciality>? DoctorSpecialities { get; set; }
}