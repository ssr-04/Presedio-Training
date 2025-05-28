using AutoMapper;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IMapper _mapper;

    public DoctorService(IDoctorRepository doctorRepository, IMapper mapper)
    {
        _doctorRepository = doctorRepository;
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
        // Business logic: Ensure email is unique for non-deleted doctors
        if (await _doctorRepository.DoctorExistsByEmailAsync(doctorDto.Email))
        {
            throw new InvalidOperationException($"Doctor with email {doctorDto.Email}' already exists");
        }
        var doctor = _mapper.Map<Doctor>(doctorDto);
        var addedDoctor = await _doctorRepository.AddDoctorAsync(doctor);
        return _mapper.Map<DoctorResponseDto>(addedDoctor);
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
        return await _doctorRepository.SoftDeleteDoctorAsync(id);
    }

    public async Task<bool> DoctorExistsAsync(int id, bool includeDeleted = false)
    {
        return await _doctorRepository.DoctorExistsAsync(id, includeDeleted);
    }

    public async Task<bool> DoctorExistsByEmailAsync(string email, bool includeDeleted = false)
    {
        return await _doctorRepository.DoctorExistsByEmailAsync(email, includeDeleted);
    }
}