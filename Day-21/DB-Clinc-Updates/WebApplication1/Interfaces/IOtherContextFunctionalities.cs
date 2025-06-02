using AutoMapper; 
public interface IOtherContextFunctionalities
{
    Task<IEnumerable<Doctor>> GetDoctorsBySpecialityNameFromSpAsync(string specialityName);
    Task<Doctor> AddDoctorTransactionalAsync(
        DoctorCreateDto doctorDto,
        IDoctorRepository doctorRepository,
        ISpecialityService specialityService,
        IDoctorSpecialityService doctorSpecialityService,
        IMapper mapper);
}
