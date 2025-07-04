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
            var proposal = await _dbSet
                .Where(p => !p.IsDeleted && p.Id == id)
                .Include(p => p.Project).ThenInclude(p => p.ProjectSkills!).ThenInclude(ps => ps.Skill)
                .Include(p => p.Freelancer).ThenInclude(f => f.FreelancerProfile!).ThenInclude(f => f.FreelancerSkills!).ThenInclude(fs => fs.Skill)
                .Include(p => p.Attachments)
                .FirstOrDefaultAsync();
            if(proposal != null)
            {
                proposal.Attachments = proposal.Attachments?
                    .Where(a => !a.IsDeleted)
                    .ToList() ?? new List<Models.File>();
            }
            return proposal;
        }

        public async Task<IEnumerable<Proposal>> GetProposalsByFreelancerAsync(Guid freelancerId, bool includeDetails = false)
        {
            IQueryable<Proposal> query = _dbSet.Where(p => !p.IsDeleted && p.FreelancerId == freelancerId);

            if (includeDetails)
            {
                query = query
                    .Include(p => p.Project)
                    .Include(p => p.Attachments); // This brings all attachments
            }

            var proposals = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            // Now filter the Attachments to include only non-deleted ones
            if (includeDetails)
            {
                foreach (var proposal in proposals)
                {
                    proposal.Attachments = proposal.Attachments?
                        .Where(a => !a.IsDeleted)
                        .ToList() ?? new List<Models.File>();
                }
            }

            return proposals;
        }

        public async Task<IEnumerable<Proposal>> GetProposalsForProjectAsync(Guid projectId, bool includeDetails = false)
        {
            IQueryable<Proposal> query = _dbSet.Where(p => !p.IsDeleted && p.ProjectId == projectId);

            if (includeDetails)
            {
                query = query.Include(p => p.Freelancer).ThenInclude(f => f.FreelancerProfile)
                                .Include(p => p.Attachments);
            }

            var proposals = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            if (includeDetails)
            {
                foreach (var proposal in proposals)
                {
                    proposal.Attachments = proposal.Attachments?
                        .Where(a => !a.IsDeleted)
                        .ToList() ?? new List<Models.File>();
                }
            }

            return proposals;
        }
    }
}