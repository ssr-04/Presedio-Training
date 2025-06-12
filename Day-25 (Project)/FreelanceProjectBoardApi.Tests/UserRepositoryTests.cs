using FluentAssertions;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Repositories;

namespace FreelanceProjectBoardApi.Tests.Repositories;

public class UserRepositoryTests : RepositoryTestBase
{
    private readonly UserRepository _userRepository;

    public UserRepositoryTests()
    {
        _userRepository = new UserRepository(_context);
    }

    protected override void SeedDatabase()
    {
        var users = new List<User>
        {
            new User {Email = "Client@test.com", Type = UserType.Client},
            new User {Email = "Freelancer1@test.com", Type = UserType.Freelancer},
            new User {Email = "Freelancer2@test.com", Type = UserType.Freelancer}
        };
        _context.Users.AddRange(users);
    }

    [Fact]
    public async Task GetAllUsersAsync_FilterByType_returnsMatchingUsers()
    {
        var filter = new UserFilter();
        filter.UserType = "Client";
        var result = await _userRepository.GetAllUsersAsync(filter, new PaginationParams());
        result.Data.Should().HaveCount(1);
        result.Data.First().Type.Should().Be(UserType.Client);
    }

    [Fact]
    public async Task GetAllUsersAsync_SearchByEmail_ReturnsMatchingUser()
    {
        var filter = new UserFilter();
        filter.SearchQuery = "freelancer";
        var result = await _userRepository.GetAllUsersAsync(filter, new PaginationParams());
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserByEmail_WithValidEmail_ReturnsValidUser()
    {
        var user = await _userRepository.GetUserByEmailAsync("freelancer1@test.com");
        user.Should().NotBeNull();
        user!.Type.Should().Be(UserType.Freelancer);
    }

    [Fact]
    public async Task GetUserByEmail_WithInvalidEmail_ReturnsNull()
    {
        var user = await _userRepository.GetUserByEmailAsync("nonexistentemail@test.com");
        user.Should().BeNull();
    }
}