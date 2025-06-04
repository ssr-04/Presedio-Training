using AutoMapper;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly ISpecialityService _specialityService;
    private readonly IDoctorSpecialityService _doctorSpecialityService;
    private readonly IOtherContextFunctionalities _otherContextFunctions;
    private readonly IUserRepository _userRepository; 
    private readonly IAuth0ManagementService _auth0ManagementService; 
    private readonly IMapper _mapper;

    public DoctorService(
        IDoctorRepository doctorRepository,
        ISpecialityService specialityService,
        IDoctorSpecialityService doctorSpecialityService,
        IOtherContextFunctionalities otherContextFunctions, // Injection for using other functionalities
        IUserRepository userRepository, // Added
        IAuth0ManagementService auth0ManagementService, 
        IMapper mapper
        )
    {
        _doctorRepository = doctorRepository;
        _specialityService = specialityService;
        _doctorSpecialityService = doctorSpecialityService;
        _otherContextFunctions = otherContextFunctions;
        _userRepository = userRepository; // Assign
        _auth0ManagementService = auth0ManagementService; // Assign
        _mapper = mapper;
    }

    public async Task<DoctorResponseDto?> GetDoctorByIdAsync(int id, bool includeDeleted = false)
    {
        var doctor = await _doctorRepository.GetDoctorByIdAsync(id, includeDeleted);
        return _mapper.Map<DoctorResponseDto>(doctor);
    }

    public async Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync(bool includeDeleted = false)
    {
        var doctors = await _doctorRepository.GetAllDoctorsAsync(includeDeleted);
        return _mapper.Map<IEnumerable<DoctorResponseDto>>(doctors);
    }

    public async Task<DoctorResponseDto> AddDoctorAsync(DoctorCreateDto doctorDto)
    {
        // 1. Checking if already exists
        if (await _userRepository.UserExistsByUsernameAsync(doctorDto.Email))
        {
            throw new InvalidOperationException($"A user account with email '{doctorDto.Email}' already exists.");
        }
        
        var existingAuth0User = await _auth0ManagementService.GetAuth0UserByEmailAsync(doctorDto.Email);
        if (existingAuth0User != null)
        {
            throw new InvalidOperationException($"A user with email '{doctorDto.Email}' already exists in Auth0.");
        }

        // 2. Creating in Auth0
        Auth0.ManagementApi.Models.User createdAuth0User;
        try
        {
            createdAuth0User = await _auth0ManagementService.CreateAuth0UserAsync(
                doctorDto.Email,
                doctorDto.Password, // Auth0 will hash this
                "Doctor" // Role to assign in Auth0's metadata
            );
        }
        catch (Exception ex)
        {
            // Log the error and rethrow or handle specific Auth0 errors
            throw new InvalidOperationException($"Failed to create user in Auth0: {ex.Message}", ex);
        }

        // 3) Creating User locally
        var localUser = new User
        {
            Username = doctorDto.Email,
            Auth0UserId = createdAuth0User.UserId,
            Role = "Doctor", // Assign the Doctor role
            IsActive = true,
            CreatedAt = DateTime.UtcNow
            // DoctorId will be set after the doctor is added
        };

        // 4. Doctor creation
        if (await _doctorRepository.DoctorExistsByEmailAsync(doctorDto.Email))
        {
            throw new InvalidOperationException($"Doctor with email '{doctorDto.Email}' already exists.");
        }
        var doctor = _mapper.Map<Doctor>(doctorDto);
        var addedDoctor = await _doctorRepository.AddDoctorAsync(doctor);

         // 3. Link the created Doctor to the User
        localUser.DoctorId = addedDoctor.Id;
        await _userRepository.AddUserAsync(localUser);

        // If provided specialities
        if (doctorDto.SpecialityNames != null && doctorDto.SpecialityNames.Any())
        {
            foreach (var specialityName in doctorDto.SpecialityNames
                                            .Select(s => s.Name)
                                            .Where(n => !string.IsNullOrWhiteSpace(n))
                                            .Distinct()) // Using Distinct to avoid duplicates
            {
                SpecialityResponseDto? speciality = null;
                try
                {
                    // find speciality by name (case-insensitive)
                    speciality = (await _specialityService.GetAllSpecialitiesAsync(includeDeleted: false))
                                 .FirstOrDefault(s => string.Equals(s.Name, specialityName, StringComparison.OrdinalIgnoreCase));
                }
                catch (Exception ex)
                {
                    // Logs the error but doesn't prevent doctor creation
                    Console.WriteLine($"Error looking up speciality '{specialityName}': {ex.Message}");
                }


                if (speciality == null)
                {
                    // Speciality doesn't exist, create it
                    try
                    {
                        var createSpecialityDto = new SpecialityCreateDto { Name = specialityName, Status = "Active" };
                        speciality = await _specialityService.AddSpecialityAsync(createSpecialityDto);
                    }
                    catch (Exception ex)
                    {
                        // Log the error. we'll just skip adding this speciality if creation fails. (not roll back)
                        Console.WriteLine($"Error creating speciality '{specialityName}': {ex.Message}");
                        continue;
                    }
                }

                // Now establishing the DoctorSpeciality relationship
                if (addedDoctor.Id > 0 && speciality != null)
                {
                    try
                    {
                        var createDoctorSpecialityDto = new DoctorSpecialityCreateDto
                        {
                            DoctorId = addedDoctor.Id,
                            SpecialityId = speciality.Id
                        };
                        await _doctorSpecialityService.AddDoctorSpecialityAsync(createDoctorSpecialityDto);
                    }
                    catch (InvalidOperationException ex) // Catches "association already exists" or "not found" from service
                    {
                        Console.WriteLine($"Warning: Could not establish Doctor-Speciality association for Doctor ID {addedDoctor.Id} and Speciality ID {speciality.Id}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error establishing Doctor-Speciality association for Doctor ID {addedDoctor.Id} and Speciality ID {speciality.Id}: {ex.Message}");
                    }
                }
            }
        }

        var finalDoctor = await GetDoctorByIdAsync(addedDoctor.Id); // Fetch with navigation properties
        if (finalDoctor == null)
        {
            throw new InvalidOperationException($"Failed to retrieve newly created doctor with ID {addedDoctor.Id} after adding specialities.");
        }

        return finalDoctor;

    }
    public async Task<DoctorResponseDto?> UpdateDoctorAsync(int id, DoctorUpdateDto doctorDto)
    {
        var existingDoctor = await _doctorRepository.GetDoctorByIdAsync(id, includeDeleted: false); // Only update nondeleted
        if (existingDoctor == null)
        {
            return null; // Or throw NotFoundException
        }
        // If email is being changed, check for uniqueness
        if (doctorDto.Email != existingDoctor.Email && await _doctorRepository.DoctorExistsByEmailAsync(doctorDto.Email, includeDeleted: false))
        {
            throw new InvalidOperationException($"Doctor with email '{doctorDto.Email}' already exists.");
        }

        _mapper.Map(doctorDto, existingDoctor);

        var updatedDoctor = await _doctorRepository.UpdateDoctorAsync(existingDoctor);
        return _mapper.Map<DoctorResponseDto>(updatedDoctor);
    }

    public async Task<bool> SoftDeleteDoctorAsync(int id)
    {
        var doctor = await _doctorRepository.GetDoctorByIdAsync(id); // Getting doctor to access email for user update
        if (doctor == null)
        {
            return false;
        }

        bool deleted = await _doctorRepository.SoftDeleteDoctorAsync(id);

        if (deleted)
        {
            // If soft-delete was successful, updating the associated user to inactive
            var user = await _userRepository.GetUserByUsernameAsync(doctor.Email);
            if (user != null && user.DoctorId == doctor.Id) // extra check
            {
                user.IsActive = false;
                await _userRepository.UpdateUserAsync(user);
            }
        }
        return deleted;

    }

    public async Task<bool> DoctorExistsAsync(int id, bool includeDeleted = false)
    {
        return await _doctorRepository.DoctorExistsAsync(id, includeDeleted);
    }

    public async Task<bool> DoctorExistsByEmailAsync(string email, bool includeDeleted = false)
    {
        return await _doctorRepository.DoctorExistsByEmailAsync(email, includeDeleted);
    }

    public async Task<IEnumerable<DoctorResponseDto>> GetDoctorsBySpecialityNameAsync(string specialityName)
    {
        var doctors = await _otherContextFunctions.GetDoctorsBySpecialityNameFromSpAsync(specialityName);
        return _mapper.Map<IEnumerable<DoctorResponseDto>>(doctors);
    }
    
    public async Task<DoctorResponseDto> AddDoctorByTransactionAsync(DoctorCreateDto doctorDto)
    {
        var doctorResponse = await _otherContextFunctions.AddDoctorTransactionalAsync(
            doctorDto,
            _doctorRepository,
            _specialityService,
            _doctorSpecialityService,
            _mapper 
        );

        var finalDoctor = await GetDoctorByIdAsync(doctorResponse.Id);
        if (finalDoctor == null)
        {
            throw new Exception($"Failed to get doctor with ID {doctorResponse.Id} after transaction add.");
        }
        return finalDoctor;
    }
}