public interface IDoctorRepository
{
    Task<IEnumerable<Doctor>> GetAllDoctorsAsync(bool includeDeleted = false);
    Task<Doctor?> GetDoctorByIdAsync(int id, bool includeDeleted = false);
    Task<Doctor> AddDoctorAsync(Doctor doctor);
    Task<Doctor?> UpdateDoctorAsync(Doctor doctor);
    Task<bool> SoftDeleteDoctorAsync(int id);
    Task<bool> DoctorExistsAsync(int id, bool includeDeleted = false);
    Task<bool> DoctorExistsByEmailAsync(string email, bool includeDeleted = false);
}