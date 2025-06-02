using AutoMapper;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository; 
    private readonly IPasswordHasher _passwordHasher;


    public PatientService(IPatientRepository patientRepository, IMapper mapper, IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _patientRepository = patientRepository;
        _mapper = mapper;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;

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

        // 1. User Account Creation Logic
        if (await _userRepository.UserExistsByUsernameAsync(patientDto.Email))
        {
            throw new InvalidOperationException($"A user account with email '{patientDto.Email}' already exists.");
        }

        var hashedPassword = _passwordHasher.HashPassword(patientDto.Password);

        var user = new User
        {
            Username = patientDto.Email,
            PasswordHash = hashedPassword,
            Role = "Patient", // Assign the Patient role
            IsActive = true,
            CreatedAt = DateTime.UtcNow
            // PatientId will be set after the patient is added and we have its ID
        };

        // 2. Patient creation
        // email is unique for non-deleted patients
        if (await _patientRepository.PatientExistsByEmailAsync(patientDto.Email))
        {
            // a custom exception for better error handling
            throw new InvalidOperationException($"Patient with email '{patientDto.Email}' already exists.");
        }

        var patient = _mapper.Map<Patient>(patientDto);
        var addedPatient = await _patientRepository.AddPatientAsync(patient);

        // 3. Link the created Patient to the User
        user.PatientId = addedPatient.Id;
        await _userRepository.AddUserAsync(user); 

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
            return false;
        }

        bool deleted = await _patientRepository.SoftDeletePatientAsync(id);

        if (deleted)
        {
            // If soft-delete was successful,
            var user = await _userRepository.GetUserByUsernameAsync(patient.Email);
            if (user != null && user.PatientId == patient.Id)
            {
                user.IsActive = false;
                await _userRepository.UpdateUserAsync(user);
            }
        }
        return deleted;
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