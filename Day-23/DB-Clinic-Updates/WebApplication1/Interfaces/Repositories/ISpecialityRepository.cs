public interface ISpecialityRepository
{
    Task<IEnumerable<Speciality>> GetAllSpecialitiesAsync(bool includeDeleted = false);
    Task<Speciality?> GetSpecialityByIdAsync(int id, bool includeDeleted = false);
    Task<Speciality> AddSpecialityAsync(Speciality speciality);
    Task<Speciality?> UpdateSpecialityAsync(Speciality speciality);
    Task<bool> SoftDeleteSpecialityAsync(int id);
    Task<bool> SpecialityExistsAsync(int id, bool includeDeleted = false);
    Task<bool> SpecialityExistsByNameAsync(string name, bool includeDeleted = false);
}