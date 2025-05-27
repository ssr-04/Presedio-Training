public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;

    public DoctorService(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync()
    {
        var doctors = await _doctorRepository.GetAllDoctorsAsync();
        return doctors.Select(d => new DoctorResponseDto
        {
            Id = d.Id,
            FirstName = d.FirstName,
            LastName = d.LastName,
            Specialty = d.Specialty,
            Email = d.Email,
            PhoneNumber = d.PhoneNumber
        });
    }

    public async Task<DoctorResponseDto?> GetDoctorByIdAsync(Guid id)
    {
        var doctor = await _doctorRepository.GetDoctorByIdAsync(id);
        if (doctor == null) return null;

        return new DoctorResponseDto
        {
            Id = doctor.Id,
            FirstName = doctor.FirstName,
            LastName = doctor.LastName,
            Specialty = doctor.Specialty,
            Email = doctor.Email,
            PhoneNumber = doctor.PhoneNumber
        };
    }

    public async Task<(DoctorResponseDto? doctor, string? error)> AddDoctorAsync(DoctorCreateDto doctorDto)
    {
        // validation for email uniqueness
        if (await _doctorRepository.DoctorExistsByEmailAsync(doctorDto.Email))
        {
            return (null, "Doctor with this email already exists.");
        }

        var doctor = new Doctor
        {
            FirstName = doctorDto.FirstName,
            LastName = doctorDto.LastName,
            Specialty = doctorDto.Specialty,
            Email = doctorDto.Email,
            PhoneNumber = doctorDto.PhoneNumber
        };

        var addedDoctor = await _doctorRepository.AddDoctorAsync(doctor);
        return (new DoctorResponseDto
        {
            Id = addedDoctor.Id,
            FirstName = addedDoctor.FirstName,
            LastName = addedDoctor.LastName,
            Specialty = addedDoctor.Specialty,
            Email = addedDoctor.Email,
            PhoneNumber = addedDoctor.PhoneNumber
        }, null);
    }

    public async Task<(DoctorResponseDto? doctor, string? error)> UpdateDoctorAsync(DoctorUpdateDto doctorDto)
    {
        var existingDoctor = await _doctorRepository.GetDoctorByIdAsync(doctorDto.Id);
        if (existingDoctor == null)
        {
            return (null, "Doctor not found.");
        }

        // if email is being changed to an existing email of another doctor
        if (!existingDoctor.Email.Equals(doctorDto.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _doctorRepository.DoctorExistsByEmailAsync(doctorDto.Email))
            {
                return (null, "Another doctor with this email already exists.");
            }
        }

        existingDoctor.FirstName = doctorDto.FirstName;
        existingDoctor.LastName = doctorDto.LastName;
        existingDoctor.Specialty = doctorDto.Specialty;
        existingDoctor.Email = doctorDto.Email;
        existingDoctor.PhoneNumber = doctorDto.PhoneNumber;

        var updatedDoctor = await _doctorRepository.UpdateDoctorAsync(existingDoctor);
        if (updatedDoctor == null) return (null, "Failed to update doctor."); //Won't happen, but just in case

        return (new DoctorResponseDto
        {
            Id = updatedDoctor.Id,
            FirstName = updatedDoctor.FirstName,
            LastName = updatedDoctor.LastName,
            Specialty = updatedDoctor.Specialty,
            Email = updatedDoctor.Email,
            PhoneNumber = updatedDoctor.PhoneNumber
        }, null);
    }

    public async Task<bool> DeleteDoctorAsync(Guid id)
    {
        // In real, prevent deletion if doctor has upcoming appointments or cancel them
        return await _doctorRepository.DeleteDoctorAsync(id);
    }
}