using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Interfaces.Repositories
{
    public interface ISkillRepository : IGenericRepository<Skill>
    {
        Task<Skill?> GetSkillByNameAsync(string name);

        Task<PageResult<Skill>> GetAllSkillsAsync(SkillFilter filter, PaginationParams pagination);
    }
}