public interface IDoctorService
{
    Task<DoctorResponseDto?> GetDoctorByIdAsync(int id, bool includeDeleted = false);
    Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync(bool includeDeleted = false);
    Task<DoctorResponseDto> AddDoctorAsync(DoctorCreateDto doctorDto);
    Task<DoctorResponseDto?> UpdateDoctorAsync(int id, DoctorUpdateDto doctorDto);
    Task<bool> SoftDeleteDoctorAsync(int id);
    Task<bool> DoctorExistsAsync(int id, bool includeDeleted = false);
    Task<bool> DoctorExistsByEmailAsync(string email, bool includeDeleted = false);
    Task<IEnumerable<DoctorResponseDto>> GetDoctorsBySpecialityNameAsync(string specialityName); //Uses stored procedures
    Task<DoctorResponseDto> AddDoctorByTransactionAsync(DoctorCreateDto doctorDto); //Uses transactions (better than AddDoctorAsync for atomicity)
}