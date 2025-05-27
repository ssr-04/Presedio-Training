public interface IDoctorService
{
    Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync();
    Task<DoctorResponseDto?> GetDoctorByIdAsync(Guid id);
    Task<(DoctorResponseDto? doctor, string? error)> AddDoctorAsync(DoctorCreateDto doctorDto);
    Task<(DoctorResponseDto? doctor, string? error)> UpdateDoctorAsync(DoctorUpdateDto doctorDto);
    Task<bool> DeleteDoctorAsync(Guid id);
}