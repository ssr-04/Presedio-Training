using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Interfaces.Repositories
{
    public interface IFreelancerSkillRepository : IGenericRepository<FreelancerSkill>
    {
        Task<IEnumerable<FreelancerSkill>> GetSkillsForFreelancerAsync(Guid freelancerProfileId);
        Task<bool> FreelancerHasSkillAsync(Guid freelancerProfileId, Guid skillId);
        Task AddRangeAsync(IEnumerable<FreelancerSkill> freelancerSkills);
    }
}