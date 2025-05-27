public interface IDoctorService
{
    Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync();
    Task<DoctorResponseDto?> GetDoctorByIdAsync(int id);
    Task<(DoctorResponseDto? doctor, string? error)> AddDoctorAsync(DoctorCreateDto doctorDto);
    Task<(DoctorResponseDto? doctor, string? error)> UpdateDoctorAsync(DoctorUpdateDto doctorDto);
    Task<bool> DeleteDoctorAsync(int id); // Soft delete
}