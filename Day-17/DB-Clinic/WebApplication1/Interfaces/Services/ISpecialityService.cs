public interface ISpecialityService
{
    Task<IEnumerable<SpecialityResponseDto>> GetAllSpecialitiesAsync();
    Task<SpecialityResponseDto?> GetSpecialityByIdAsync(int id);
    Task<(SpecialityResponseDto? speciality, string? error)> AddSpecialityAsync(SpecialityCreateDto specialityDto);
    Task<(SpecialityResponseDto? speciality, string? error)> UpdateSpecialityAsync(SpecialityUpdateDto specialityDto);
    Task<bool> DeleteSpecialityAsync(int id); // Soft delete
}