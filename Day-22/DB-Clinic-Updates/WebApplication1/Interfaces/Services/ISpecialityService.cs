public interface ISpecialityService
{
    Task<SpecialityResponseDto?> GetSpecialityByIdAsync(int id, bool includeDeleted = false);
    Task<IEnumerable<SpecialityResponseDto>> GetAllSpecialitiesAsync(bool includeDeleted = false);
    Task<SpecialityResponseDto> AddSpecialityAsync(SpecialityCreateDto specialityDto);
    Task<SpecialityResponseDto?> UpdateSpecialityAsync(int id, SpecialityUpdateDto specialityDto);
    Task<bool> SoftDeleteSpecialityAsync(int id);
    Task<bool> SpecialityExistsAsync(int id, bool includeDeleted = false);
    Task<bool> SpecialityExistsByNameAsync(string name, bool includeDeleted = false);
}
