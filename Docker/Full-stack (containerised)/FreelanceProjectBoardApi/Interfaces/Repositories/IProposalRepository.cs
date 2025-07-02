using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Interfaces.Repositories
{
    public interface IProposalRepository : IGenericRepository<Proposal>
    {
        Task<Proposal?> GetProposalDetailsAsync(Guid id);
        Task<IEnumerable<Proposal>> GetProposalsForProjectAsync(Guid projectId, bool includeDetails = false);
        Task<IEnumerable<Proposal>> GetProposalsByFreelancerAsync(Guid freelancerId, bool includeDetails = false);
    }
}