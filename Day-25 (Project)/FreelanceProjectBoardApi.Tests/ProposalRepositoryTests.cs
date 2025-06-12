using FluentAssertions;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Repositories;

namespace FreelanceProjectBoardApi.Tests.Repositories;

public class ProposalRepositoryTests : RepositoryTestBase
{
    private readonly ProposalRepository _proposalRepository;
    private static readonly Guid ClientId = Guid.NewGuid();
    private static readonly Guid FreelancerId = Guid.NewGuid();
    private static readonly Guid ProjectId = Guid.NewGuid();
    private static readonly Guid ProposalId = Guid.NewGuid();

    public ProposalRepositoryTests()
    {
        _proposalRepository = new ProposalRepository(_context);
    }

    protected override void SeedDatabase()
    {
        var client = new User { Id = ClientId, Email = "client@test.com", Type = UserType.Client };
        var freelancer = new User { Id = FreelancerId, Email = "freelancer@test.com", Type = UserType.Freelancer };
        _context.Users.AddRange(client, freelancer);

        var project = new Project { Id = ProjectId, Title = "Test Project", ClientId = ClientId, Client = client };
        _context.Projects.Add(project);

        var proposal = new Proposal { Id = ProposalId, ProjectId = ProjectId, Project = project, FreelancerId = FreelancerId, Freelancer = freelancer, CoverLetter = "I deliver the best." };
        _context.Proposals.Add(proposal);
    }

    [Fact]
    public async Task GetProposalsForProjectAsync_WithValidProjectId_ReturnsProposals()
    {
        var proposals = await _proposalRepository.GetProposalsForProjectAsync(ProjectId);
        proposals.Should().ContainSingle();
    }

    [Fact]
    public async Task GetProposalsByFreelancerAsync_WithValidFreelancerId_ReturnsProposals()
    {
        var proposals = await _proposalRepository.GetProposalsByFreelancerAsync(FreelancerId);
        proposals.Should().ContainSingle();
    }

    [Fact]
    public async Task GetProposalDetailsAsync_WithValidId_ReturnsProposalWithIncludes()
    {
        var proposals = await _proposalRepository.GetProposalDetailsAsync(ProposalId);
        proposals.Should().NotBeNull();
        proposals!.Project.Should().NotBeNull();
        proposals.Freelancer.Should().NotBeNull();
    }
}
