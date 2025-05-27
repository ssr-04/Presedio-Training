public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetAllPatientsAsync();
    Task<Patient?> GetPatientByIdAsync(Guid id);
    Task<Patient> AddPatientAsync(Patient patient);
    Task<Patient?> UpdatePatientAsync(Patient patient);
    Task<bool> DeletePatientAsync(Guid id);
    Task<bool> PatientExistsAsync(Guid id);
    Task<bool> PatientExistsByEmailAsync(string email);
}