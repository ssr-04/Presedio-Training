public interface IDoctorRepository
{
    Task<IEnumerable<Doctor>> GetAllDoctorsAsync();
    Task<Doctor?> GetDoctorByIdAsync(Guid id);
    Task<Doctor> AddDoctorAsync(Doctor doctor);
    Task<Doctor?> UpdateDoctorAsync(Doctor doctor);
    Task<bool> DeleteDoctorAsync(Guid id);
    Task<bool> DoctorExistsAsync(Guid id);
    Task<bool> DoctorExistsByEmailAsync(string email);
}