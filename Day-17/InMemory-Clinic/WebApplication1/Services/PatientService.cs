public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;

    public PatientService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<IEnumerable<PatientResponseDto>> GetAllPatientsAsync()
    {
        var patients = await _patientRepository.GetAllPatientsAsync();
        return patients.Select(p => new PatientResponseDto
        {
            Id = p.Id,
            FirstName = p.FirstName,
            LastName = p.LastName,
            DateOfBirth = p.DateOfBirth,
            Email = p.Email,
            PhoneNumber = p.PhoneNumber
        });
    }

    public async Task<PatientResponseDto?> GetPatientByIdAsync(Guid id)
    {
        var patient = await _patientRepository.GetPatientByIdAsync(id);
        if (patient == null) return null;

        return new PatientResponseDto
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            DateOfBirth = patient.DateOfBirth,
            Email = patient.Email,
            PhoneNumber = patient.PhoneNumber
        };
    }

    public async Task<(PatientResponseDto? patient, string? error)> AddPatientAsync(PatientCreateDto patientDto)
    {
        // Additional validation (date in future, email uniqueness)
        if (patientDto.DateOfBirth > DateTime.Now)
        {
            return (null, "Date of Birth cannot be in future.");
        }
        if (await _patientRepository.PatientExistsByEmailAsync(patientDto.Email))
        {
            return (null, "Patient with this email already exists.");
        }

        var patient = new Patient
        {
            FirstName = patientDto.FirstName,
            LastName = patientDto.LastName,
            DateOfBirth = patientDto.DateOfBirth,
            Email = patientDto.Email,
            PhoneNumber = patientDto.PhoneNumber
        };

        var addedPatient = await _patientRepository.AddPatientAsync(patient);
        return (new PatientResponseDto
        {
            Id = addedPatient.Id,
            FirstName = addedPatient.FirstName,
            LastName = addedPatient.LastName,
            DateOfBirth = addedPatient.DateOfBirth,
            Email = addedPatient.Email,
            PhoneNumber = addedPatient.PhoneNumber
        }, null);
    }

    public async Task<(PatientResponseDto? patient, string? error)> UpdatePatientAsync(PatientUpdateDto patientDto)
    {
        // Additional validation
        if (patientDto.DateOfBirth > DateTime.Now)
        {
            return (null, "Date of Birth cannot be in future.");
        }

        var existingPatient = await _patientRepository.GetPatientByIdAsync(patientDto.Id);
        if (existingPatient == null)
        {
            return (null, "Patient not found.");
        }

        // if email is being changed is existing email of another patient
        if (!existingPatient.Email.Equals(patientDto.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _patientRepository.PatientExistsByEmailAsync(patientDto.Email))
            {
                return (null, "Another patient with this email already exists.");
            }
        }

        existingPatient.FirstName = patientDto.FirstName;
        existingPatient.LastName = patientDto.LastName;
        existingPatient.DateOfBirth = patientDto.DateOfBirth;
        existingPatient.Email = patientDto.Email;
        existingPatient.PhoneNumber = patientDto.PhoneNumber;

        var updatedPatient = await _patientRepository.UpdatePatientAsync(existingPatient);
        if (updatedPatient == null) return (null, "Failed to update patient.");

        return (new PatientResponseDto
        {
            Id = updatedPatient.Id,
            FirstName = updatedPatient.FirstName,
            LastName = updatedPatient.LastName,
            DateOfBirth = updatedPatient.DateOfBirth,
            Email = updatedPatient.Email,
            PhoneNumber = updatedPatient.PhoneNumber
        }, null);
    }

    public async Task<bool> DeletePatientAsync(Guid id)
    {
        // In a real we want to check for associated appointments
        // to decide soft delete, hard delete, or prevent deletion.
        // For now, let's just delete from the patient repository alone.
        return await _patientRepository.DeletePatientAsync(id);
    }
}