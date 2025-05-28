using AutoMapper;

public class SpecialityService : ISpecialityService
{
    private readonly ISpecialityRepository _specialityRepository;
    private readonly IDoctorSpecialityRepository _doctorSpecialityRepository;
    private readonly IMapper _mapper;

    public SpecialityService(ISpecialityRepository specialityRepository, IDoctorSpecialityRepository doctorSpecialityRepository, IMapper mapper)
    {
        _specialityRepository = specialityRepository;
        _doctorSpecialityRepository = doctorSpecialityRepository;
        _mapper = mapper;
    }

    public async Task<SpecialityResponseDto?> GetSpecialityByIdAsync(int id, bool includeDeleted = false)
    {
        var speciality = await _specialityRepository.GetSpecialityByIdAsync(id, includeDeleted);
        return _mapper.Map<SpecialityResponseDto>(speciality);
    }

    public async Task<IEnumerable<SpecialityResponseDto>> GetAllSpecialitiesAsync(bool includeDeleted = false)
    {
        var specialities = await _specialityRepository.GetAllSpecialitiesAsync(includeDeleted);
        return _mapper.Map<IEnumerable<SpecialityResponseDto>>(specialities);
    }

    public async Task<SpecialityResponseDto> AddSpecialityAsync(SpecialityCreateDto specialityDto)
    {
        // ame is unique for non-deleted specialities
        if (await _specialityRepository.SpecialityExistsByNameAsync(specialityDto.Name))
        {
            throw new InvalidOperationException($"Speciality with name '{specialityDto.Name}' already exists.");
        }

        var speciality = _mapper.Map<Speciality>(specialityDto);
        var addedSpeciality = await _specialityRepository.AddSpecialityAsync(speciality);
        return _mapper.Map<SpecialityResponseDto>(addedSpeciality);
    }

    public async Task<SpecialityResponseDto?> UpdateSpecialityAsync(int id, SpecialityUpdateDto specialityDto)
    {
        var existingSpeciality = await _specialityRepository.GetSpecialityByIdAsync(id, includeDeleted: false); // Only update non-deleted
        if (existingSpeciality == null)
        {
            return null; 
        }

        // If name is being changed, checking for uniqueness
        if (specialityDto.Name != existingSpeciality.Name && await _specialityRepository.SpecialityExistsByNameAsync(specialityDto.Name, includeDeleted: false))
        {
            throw new InvalidOperationException($"Speciality with name '{specialityDto.Name}' already exists.");
        }

        _mapper.Map(specialityDto, existingSpeciality);

        var updatedSpeciality = await _specialityRepository.UpdateSpecialityAsync(existingSpeciality);
        return _mapper.Map<SpecialityResponseDto>(updatedSpeciality);
    }

    public async Task<bool> SoftDeleteSpecialityAsync(int id)
    {
        var speciality = await _specialityRepository.GetSpecialityByIdAsync(id); 
        if (speciality == null)
        {
            return false; 
        }

        // Prevent soft-deleting a speciality if it's currently assigned to any active doctors
        var hasActiveDoctorAssociations = await _doctorSpecialityRepository.GetDoctorSpecialitiesBySpecialityIdAsync(id)
                                                                            .ContinueWith(t => t.Result.Any()); // Check if any associations exist
        if (hasActiveDoctorAssociations)
        {
            throw new InvalidOperationException($"Cannot soft delete Speciality ID {id} as it is currently associated with active doctors.");
        }

        return await _specialityRepository.SoftDeleteSpecialityAsync(id);
    }

    public async Task<bool> SpecialityExistsAsync(int id, bool includeDeleted = false)
    {
        return await _specialityRepository.SpecialityExistsAsync(id, includeDeleted);
    }

    public async Task<bool> SpecialityExistsByNameAsync(string name, bool includeDeleted = false)
    {
        return await _specialityRepository.SpecialityExistsByNameAsync(name, includeDeleted);
    }
}