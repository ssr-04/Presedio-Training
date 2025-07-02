using FreelanceProjectBoardApi.Context;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FreelanceProjectBoardApi.Repositories
{
    public class ProjectSkillRepository : GenericRepository<ProjectSkill>, IProjectSkillRepository
    {
        public ProjectSkillRepository(FreelanceContext context) : base(context)
        {

        }

        public async Task<IEnumerable<ProjectSkill>> GetSkillsForProjectAsync(Guid projectId)
        {
            return await _dbSet
                    .Where(ps => !ps.IsDeleted && ps.ProjectId == projectId)
                    .Include(ps => ps.Skill)
                    .ToListAsync();
        }

        public async Task<bool> ProjectRequiresSkillAsync(Guid projectId, Guid skillId)
        {
            return await _dbSet
                    .AnyAsync(ps => !ps.IsDeleted && ps.ProjectId == projectId && ps.SkillId == skillId);
        }
        
        public async Task AddRangeAsync(IEnumerable<ProjectSkill> projectSkills)
        {
            await _dbSet.AddRangeAsync(projectSkills);
        }
    }
}