using AutoMapper;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IMapper _mapper;

    public PatientService(IPatientRepository patientRepository, IMapper mapper)
    {
        _patientRepository = patientRepository;
        _mapper = mapper;
    }

    public async Task<PatientResponseDto?> GetPatientByIdAsync(int id, bool includeDeleted = false)
    {
        var patient = await _patientRepository.GetPatientByIdAsync(id, includeDeleted);
        return _mapper.Map<PatientResponseDto>(patient);
    }

    public async Task<IEnumerable<PatientResponseDto>> GetAllPatientsAsync(bool includeDeleted = false)
    {
        var patients = await _patientRepository.GetAllPatientsAsync(includeDeleted);
        return _mapper.Map<IEnumerable<PatientResponseDto>>(patients);
    }

    public async Task<PatientResponseDto> AddPatientAsync(PatientCreateDto patientDto)
    {
        // email is unique for non-deleted patients
        if (await _patientRepository.PatientExistsByEmailAsync(patientDto.Email))
        {
            // a custom exception for better error handling
            throw new InvalidOperationException($"Patient with email '{patientDto.Email}' already exists.");
        }

        var patient = _mapper.Map<Patient>(patientDto);
        var addedPatient = await _patientRepository.AddPatientAsync(patient);
        return _mapper.Map<PatientResponseDto>(addedPatient);
    }

    public async Task<PatientResponseDto?> UpdatePatientAsync(int id, PatientUpdateDto patientDto)
    {
        // the patient exists and isn't soft-deleted
        var existingPatient = await _patientRepository.GetPatientByIdAsync(id, includeDeleted: false); 
        if (existingPatient == null)
        {
            return null; 
        }

        // If email is changed, checking for uniqueness
        if (patientDto.Email != existingPatient.Email && await _patientRepository.PatientExistsByEmailAsync(patientDto.Email, includeDeleted: false))
        {
            throw new InvalidOperationException($"Patient with email '{patientDto.Email}' already exists.");
        }

        // Mapping DTO properties onto existing entity
        _mapper.Map(patientDto, existingPatient);

        var updatedPatient = await _patientRepository.UpdatePatientAsync(existingPatient);
        return _mapper.Map<PatientResponseDto>(updatedPatient);
    }

    public async Task<bool> SoftDeletePatientAsync(int id)
    {
        // if patient exists before attempting to delete (repo handles it too)
        var patient = await _patientRepository.GetPatientByIdAsync(id); 
        if (patient == null)
        {
            return false; // Patient not found or already deleted
        }

        return await _patientRepository.SoftDeletePatientAsync(id);
    }

    public async Task<bool> PatientExistsAsync(int id, bool includeDeleted = false)
    {
        return await _patientRepository.PatientExistsAsync(id, includeDeleted);
    }

    public async Task<bool> PatientExistsByEmailAsync(string email, bool includeDeleted = false)
    {
        return await _patientRepository.PatientExistsByEmailAsync(email, includeDeleted);
    }
}