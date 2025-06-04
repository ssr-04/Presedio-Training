using AutoMapper;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IAuth0ManagementService _auth0ManagementService; 


    public PatientService(IPatientRepository patientRepository, IMapper mapper, IUserRepository userRepository, 
                            IAuth0ManagementService auth0ManagementService )
    {
        _patientRepository = patientRepository;
        _mapper = mapper;
        _userRepository = userRepository;
        _auth0ManagementService = auth0ManagementService; 

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

        // 1. Check if a local user or Auth0 user already exists
        if (await _userRepository.UserExistsByUsernameAsync(patientDto.Email))
        {
            throw new InvalidOperationException($"A local user account with email '{patientDto.Email}' already exists. Cannot create patient.");
        }
        var existingAuth0User = await _auth0ManagementService.GetAuth0UserByEmailAsync(patientDto.Email);
        if (existingAuth0User != null)
        {
            throw new InvalidOperationException($"A user with email '{patientDto.Email}' already exists in Auth0.");
        }

        // 2. Create user in Auth0
        Auth0.ManagementApi.Models.User createdAuth0User;
        try
        {
            createdAuth0User = await _auth0ManagementService.CreateAuth0UserAsync(
                patientDto.Email,
                patientDto.Password,
                "Patient" // Role to assign in Auth0's metadata
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create user in Auth0: {ex.Message}", ex);
        }

        // 3. Patient creation
        // email is unique for non-deleted patients
        if (await _patientRepository.PatientExistsByEmailAsync(patientDto.Email))
        {
            // a custom exception for better error handling
            throw new InvalidOperationException($"Patient with email '{patientDto.Email}' already exists.");
        }

        var patient = _mapper.Map<Patient>(patientDto);
        var addedPatient = await _patientRepository.AddPatientAsync(patient);

        // 4. Create local User record linked to Auth0 user
        var localUser = new User
        {
            Username = patientDto.Email,
            Auth0UserId = createdAuth0User.UserId, // Store Auth0's unique user ID
            Role = "Patient", // Assign the Patient role locally
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            PatientId = addedPatient.Id
        };
        await _userRepository.AddUserAsync(localUser);

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