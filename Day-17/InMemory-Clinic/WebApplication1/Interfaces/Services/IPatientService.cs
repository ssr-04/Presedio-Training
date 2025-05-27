public interface IPatientService
{
    Task<IEnumerable<PatientResponseDto>> GetAllPatientsAsync();
    Task<PatientResponseDto?> GetPatientByIdAsync(Guid id);
    Task<(PatientResponseDto? patient, string? error)> AddPatientAsync(PatientCreateDto patientDto);
    Task<(PatientResponseDto? patient, string? error)> UpdatePatientAsync(PatientUpdateDto patientDto);
    Task<bool> DeletePatientAsync(Guid id);
}