using Microsoft.EntityFrameworkCore;
using AutoMapper; 
public class OtherContextFunctionalities : IOtherContextFunctionalities
{
    private readonly ClinicContext _context;

    public OtherContextFunctionalities(ClinicContext context)
    {
        _context = context;
    }

    /*
        - It cn be done in repository of doctor too but just another way of doing it...
    */
    public async Task<IEnumerable<Doctor>> GetDoctorsBySpecialityNameFromSpAsync(string specialityName)
    {

        var doctors = await _context.Doctors
                                    .FromSqlRaw("SELECT * FROM get_doctors_by_specialityName({0})", specialityName)
                                    .ToListAsync();

        return doctors;
    }
    
    public async Task<Doctor> AddDoctorTransactionalAsync(
        DoctorCreateDto doctorDto,
        IDoctorRepository doctorRepository,
        ISpecialityService specialityService,
        IDoctorSpecialityService doctorSpecialityService,
        IMapper mapper)
    {
        // Uses a transaction for atomicity
        using var transaction = await _context.Database.BeginTransactionAsync();
        Doctor? addedDoctor = null;

        try
        {
            //1. Checking for duplicate email
            if (await doctorRepository.DoctorExistsByEmailAsync(doctorDto.Email))
            {
                throw new InvalidOperationException($"Doctor with email '{doctorDto.Email}' already exists.");
            }

            var doctor = mapper.Map<Doctor>(doctorDto);

            //2. Add the Doctor to the repository
            addedDoctor = await doctorRepository.AddDoctorAsync(doctor); 

            //3. Specialities and DoctorSpecialities
            if (doctorDto.SpecialityNames != null && doctorDto.SpecialityNames.Any())
            {
                foreach (var specialityName in doctorDto.SpecialityNames
                                            .Select(s => s.Name)
                                            .Where(n => !string.IsNullOrWhiteSpace(n))
                                            .Distinct())
                {
                    SpecialityResponseDto? speciality = null; 
                    try
                    {
                        // finding the speciality by name (case-insensitive)
                        speciality = (await specialityService.GetAllSpecialitiesAsync(includeDeleted: false))
                                     .FirstOrDefault(s => string.Equals(s.Name, specialityName, StringComparison.OrdinalIgnoreCase));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error finding speciality '{specialityName}': {ex.Message}");
                        throw new Exception($"Failed to find speciality '{specialityName}'. Transaction is rolled back.", ex);
                    }

                    if (speciality == null)
                    {
                        // if speciality doesn't exist, create
                        try
                        {
                            var createSpecialityDto = new SpecialityCreateDto { Name = specialityName, Status = "Active" };
                            speciality = await specialityService.AddSpecialityAsync(createSpecialityDto);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error creating speciality '{specialityName}': {ex.Message}");
                            throw new Exception($"Failed to create speciality '{specialityName}'. Transaction is rolled back.", ex);
                        }
                    }

                    // DoctorSpeciality relationship
                    if (addedDoctor.Id > 0 && speciality != null)
                    {
                        try
                        {
                            var createDoctorSpecialityDto = new DoctorSpecialityCreateDto
                            {
                                DoctorId = addedDoctor.Id,
                                SpecialityId = speciality.Id
                            };
                            await doctorSpecialityService.AddDoctorSpecialityAsync(createDoctorSpecialityDto);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Failed to establish Doctor-Speciality association for Doctor ID {addedDoctor.Id} and Speciality ID {speciality.Id}. Transaction is rolled back.", ex);
                        }
                    }
                }
            }

            // Commiting if all operations succeed
            await transaction.CommitAsync();
            Console.WriteLine("Doctor, specialities, and associations created successfully using transaction.");
        }
        catch (Exception ex)
        {
            // Rollbackr
            await transaction.RollbackAsync();
            Console.WriteLine($"Transaction rolled back due to error: {ex.Message}");
            throw;
        }

        if (addedDoctor == null)
        {
            throw new Exception("Doctor not be added during the transaction.");
        }
        return addedDoctor;
    }

}
