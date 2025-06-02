public interface IDoctorSpecialityService
{
    Task<IEnumerable<DoctorSpecialityResponseDto>> GetAllDoctorSpecialitiesAsync();
    Task<DoctorSpecialityResponseDto?> GetDoctorSpecialityBySerialNumberAsync(int serialNumber);
    Task<IEnumerable<DoctorSpecialityResponseDto>> GetDoctorSpecialitiesByDoctorIdAsync(int doctorId);
    Task<IEnumerable<DoctorSpecialityResponseDto>> GetDoctorSpecialitiesBySpecialityIdAsync(int specialityId);
    Task<DoctorSpecialityResponseDto> AddDoctorSpecialityAsync(DoctorSpecialityCreateDto doctorSpecialityDto);
    Task<bool> RemoveDoctorSpecialityAsync(int serialNumber);
}