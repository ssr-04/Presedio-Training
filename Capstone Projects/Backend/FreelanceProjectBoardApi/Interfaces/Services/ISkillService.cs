// Services/Interfaces/ISkillService.cs
using FreelanceProjectBoardApi.DTOs.Skills;
using FreelanceProjectBoardApi.Helpers;
using System.Collections.Generic;

namespace FreelanceProjectBoardApi.Services.Interfaces
{
    public interface ISkillService
    {
        Task<SkillDto?> CreateSkillAsync(CreateSkillDto createDto);
        Task<SkillDto?> GetSkillByIdAsync(Guid id);
        Task<SkillDto?> GetSkillByNameAsync(string name);
        Task<PageResult<SkillDto>> GetAllSkillsAsync(SkillFilter filter, PaginationParams pagination);
        Task<bool> DeleteSkillAsync(Guid id);
    }
}