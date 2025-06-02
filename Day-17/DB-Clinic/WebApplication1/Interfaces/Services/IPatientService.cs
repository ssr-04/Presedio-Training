
public interface IPatientService
{
    Task<PatientResponseDto?> GetPatientByIdAsync(int id, bool includeDeleted = false);
    Task<IEnumerable<PatientResponseDto>> GetAllPatientsAsync(bool includeDeleted = false);
    Task<PatientResponseDto> AddPatientAsync(PatientCreateDto patientDto);
    Task<PatientResponseDto?> UpdatePatientAsync(int id, PatientUpdateDto patientDto);
    Task<bool> SoftDeletePatientAsync(int id);
    Task<bool> PatientExistsAsync(int id, bool includeDeleted = false);
    Task<bool> PatientExistsByEmailAsync(string email, bool includeDeleted = false);
}