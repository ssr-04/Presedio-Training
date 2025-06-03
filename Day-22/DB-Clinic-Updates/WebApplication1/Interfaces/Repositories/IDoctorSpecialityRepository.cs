public interface IDoctorSpecialityRepository
{
    Task<IEnumerable<DoctorSpeciality>> GetAllDoctorSpecialitiesAsync();
    Task<DoctorSpeciality?> GetDoctorSpecialityBySerialNumberAsync(int serialNumber);
    Task<IEnumerable<DoctorSpeciality>> GetDoctorSpecialitiesByDoctorIdAsync(int doctorId);
    Task<IEnumerable<DoctorSpeciality>> GetDoctorSpecialitiesBySpecialityIdAsync(int specialityId);
    Task<DoctorSpeciality> AddDoctorSpecialityAsync(DoctorSpeciality doctorSpeciality);
    Task<bool> RemoveDoctorSpecialityAsync(int serialNumber);
    Task<bool> DoctorSpecialityExistsAsync(int doctorId, int specialityId);
}