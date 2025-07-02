using FreelanceProjectBoardApi.Context;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FreelanceProjectBoardApi.Repositories
{
    public class ProposalRepository : GenericRepository<Proposal>, IProposalRepository
    {
        public ProposalRepository(FreelanceContext context) : base(context)
        {

        }
        public async Task<Proposal?> GetProposalDetailsAsync(Guid id)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.Id == id)
                .Include(p => p.Project).ThenInclude(p => p.ProjectSkills!).ThenInclude(ps => ps.Skill)
                .Include(p => p.Freelancer).ThenInclude(f => f.FreelancerProfile!).ThenInclude(f => f.FreelancerSkills!).ThenInclude(fs => fs.Skill)
                .Include(p => p.Attachments)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Proposal>> GetProposalsByFreelancerAsync(Guid freelancerId, bool includeDetails = false)
        {
            IQueryable<Proposal> query = _dbSet.Where(p => !p.IsDeleted && p.FreelancerId == freelancerId);

            if (includeDetails)
            {
                query = query.Include(p => p.Project)
                            .Include(p => p.Attachments);
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Proposal>> GetProposalsForProjectAsync(Guid projectId, bool includeDetails = false)
        {
            IQueryable<Proposal> query = _dbSet.Where(p => !p.IsDeleted && p.ProjectId == projectId);

            if (includeDetails)
            {
                query = query.Include(p => p.Freelancer).ThenInclude(f => f.FreelancerProfile)
                                .Include(p => p.Attachments);
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }
    }
}