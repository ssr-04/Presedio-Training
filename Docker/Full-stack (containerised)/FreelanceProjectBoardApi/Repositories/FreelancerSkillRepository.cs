using FreelanceProjectBoardApi.Context;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FreelanceProjectBoardApi.Repositories
{
    public class FreelancerSkillRepository : GenericRepository<FreelancerSkill>, IFreelancerSkillRepository
    {
        public FreelancerSkillRepository(FreelanceContext context) : base(context)
        {

        }

        public async Task<bool> FreelancerHasSkillAsync(Guid freelancerProfileId, Guid skillId)
        {
            return await _dbSet.AnyAsync(fs => !fs.IsDeleted && fs.FreelancerProfileId == freelancerProfileId && fs.SkillId == skillId);
        }

        public async Task<IEnumerable<FreelancerSkill>> GetSkillsForFreelancerAsync(Guid freelancerProfileId)
        {
            var result = await _dbSet
                .Where(fs => !fs.IsDeleted && fs.FreelancerProfileId == freelancerProfileId)
                .Include(fs => fs.Skill)
                .ToListAsync();
            System.Console.WriteLine($"Hit-1 {freelancerProfileId} {result.Count()}");
            return result;
        }

        public async Task AddRangeAsync(IEnumerable<FreelancerSkill> freelancerSkills)
        {
            await _dbSet.AddRangeAsync(freelancerSkills);
        }

        public async Task DeleteFreelancerSkills(Guid freelancerProfileId)
        {
            var freelancerSkills = await _dbSet
                .Where(fs => !fs.IsDeleted && fs.FreelancerProfileId == freelancerProfileId)
                .Include(fs => fs.Skill)
                .ToListAsync();

            _dbSet.RemoveRange(freelancerSkills); 
            await _context.SaveChangesAsync();   
        }
    }
}