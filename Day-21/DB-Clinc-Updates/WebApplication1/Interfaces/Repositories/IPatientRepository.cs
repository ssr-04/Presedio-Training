public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetAllPatientsAsync(bool includeDeleted = false);
    Task<Patient?> GetPatientByIdAsync(int id, bool includeDeleted = false);
    Task<Patient> AddPatientAsync(Patient patient);
    Task<Patient?> UpdatePatientAsync(Patient patient);
    Task<bool> SoftDeletePatientAsync(int id); // soft delete by changing status
    Task<bool> PatientExistsAsync(int id, bool includeDeleted = false);
    Task<bool> PatientExistsByEmailAsync(string email, bool includeDeleted = false);
}