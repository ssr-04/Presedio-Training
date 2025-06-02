
using AutoMapper;

public class DoctorSpecialityService : IDoctorSpecialityService
{
    private readonly IDoctorSpecialityRepository _doctorSpecialityRepository;
    private readonly IDoctorRepository _doctorRepository;  
    private readonly ISpecialityRepository _specialityRepository; 
    private readonly IMapper _mapper;

    public DoctorSpecialityService(
        IDoctorSpecialityRepository doctorSpecialityRepository,
        IDoctorRepository doctorRepository,
        ISpecialityRepository specialityRepository,
        IMapper mapper)
    {
        _doctorSpecialityRepository = doctorSpecialityRepository;
        _doctorRepository = doctorRepository;
        _specialityRepository = specialityRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DoctorSpecialityResponseDto>> GetAllDoctorSpecialitiesAsync()
    {
        var doctorSpecialities = await _doctorSpecialityRepository.GetAllDoctorSpecialitiesAsync();
        return _mapper.Map<IEnumerable<DoctorSpecialityResponseDto>>(doctorSpecialities);
    }

    public async Task<DoctorSpecialityResponseDto?> GetDoctorSpecialityBySerialNumberAsync(int serialNumber)
    {
        var doctorSpeciality = await _doctorSpecialityRepository.GetDoctorSpecialityBySerialNumberAsync(serialNumber);
        return _mapper.Map<DoctorSpecialityResponseDto>(doctorSpeciality);
    }

    public async Task<IEnumerable<DoctorSpecialityResponseDto>> GetDoctorSpecialitiesByDoctorIdAsync(int doctorId)
    {
        var doctorSpecialities = await _doctorSpecialityRepository.GetDoctorSpecialitiesByDoctorIdAsync(doctorId);
        return _mapper.Map<IEnumerable<DoctorSpecialityResponseDto>>(doctorSpecialities);
    }

    public async Task<IEnumerable<DoctorSpecialityResponseDto>> GetDoctorSpecialitiesBySpecialityIdAsync(int specialityId)
    {
        var doctorSpecialities = await _doctorSpecialityRepository.GetDoctorSpecialitiesBySpecialityIdAsync(specialityId);
        return _mapper.Map<IEnumerable<DoctorSpecialityResponseDto>>(doctorSpecialities);
    }

    public async Task<DoctorSpecialityResponseDto> AddDoctorSpecialityAsync(DoctorSpecialityCreateDto doctorSpecialityDto)
    {
        // Doctor and Speciality exist and are not deleted
        var doctorExists = await _doctorRepository.DoctorExistsAsync(doctorSpecialityDto.DoctorId);
        var specialityExists = await _specialityRepository.SpecialityExistsAsync(doctorSpecialityDto.SpecialityId);

        if (!doctorExists)
        {
            throw new ArgumentException($"Doctor with ID {doctorSpecialityDto.DoctorId} not found or is deleted.");
        }
        if (!specialityExists)
        {
            throw new ArgumentException($"Speciality with ID {doctorSpecialityDto.SpecialityId} not found or is deleted.");
        }

        //  duplicate associations
        if (await _doctorSpecialityRepository.DoctorSpecialityExistsAsync(doctorSpecialityDto.DoctorId, doctorSpecialityDto.SpecialityId))
        {
            throw new InvalidOperationException($"Association between Doctor ID {doctorSpecialityDto.DoctorId} and Speciality ID {doctorSpecialityDto.SpecialityId} already exists.");
        }

        var doctorSpeciality = _mapper.Map<DoctorSpeciality>(doctorSpecialityDto);
        var addedDoctorSpeciality = await _doctorSpecialityRepository.AddDoctorSpecialityAsync(doctorSpeciality);
        return _mapper.Map<DoctorSpecialityResponseDto>(addedDoctorSpeciality);
    }

    public async Task<bool> RemoveDoctorSpecialityAsync(int serialNumber)
    {
        return await _doctorSpecialityRepository.RemoveDoctorSpecialityAsync(serialNumber);
    }
}