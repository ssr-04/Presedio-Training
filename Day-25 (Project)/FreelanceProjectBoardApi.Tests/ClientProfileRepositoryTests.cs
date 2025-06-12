using FluentAssertions;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Repositories;

namespace FreelanceProjectBoardApi.Tests.Repositories;

public class ClientProfileRepositoryTests : RepositoryTestBase
{
    private readonly ClientProfileRepository _clientProfileRepository;
    private static readonly Guid ClientUserId = Guid.NewGuid();
    private static readonly Guid ClientProfileId = Guid.NewGuid();

    public ClientProfileRepositoryTests()
    {
        _clientProfileRepository = new ClientProfileRepository(_context);
    }

    protected override void SeedDatabase()
    {
        var user = new User { Id = ClientUserId, Email = "client@corp.com", Type = UserType.Client };
        _context.Users.Add(user);

        var profile = new ClientProfile { Id = ClientProfileId, UserId = user.Id, User = user, CompanyName = "Some Corp Inc." };
        _context.ClientProfiles.Add(profile);
    }

    [Fact]
    public async Task GetAllClientProfileAsync_SearchByCompanyName_ReturnsMatching()
    {
        var filter = new ClientFilter();
        filter.SearchQuery = "Some corp";
        var result = await _clientProfileRepository.GetAllClientProfileAsync(filter, new PaginationParams());
        result.Data.Should().ContainSingle();
    }

    [Fact]
    public async Task GetClientProfileDetailsAsync_WithValidId_ReturnsProfileWithUser()
    {
        var profile = await _clientProfileRepository.GetClientProfileDetailsAsync(ClientProfileId);
        profile.Should().NotBeNull();
        profile!.User.Should().NotBeNull();
        profile.User.Email.Should().Be("client@corp.com");
    }
}