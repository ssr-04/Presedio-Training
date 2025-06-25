using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Interfaces.Repositories
{
    public interface IProjectSkillRepository : IGenericRepository<ProjectSkill>
    {
        Task<IEnumerable<ProjectSkill>> GetSkillsForProjectAsync(Guid projectId);
        Task<bool> ProjectRequiresSkillAsync(Guid projectId, Guid skillId);
        Task AddRangeAsync(IEnumerable<ProjectSkill> projectSkills);
        Task DeleteProjectSkills(Guid projectId);
    }
}