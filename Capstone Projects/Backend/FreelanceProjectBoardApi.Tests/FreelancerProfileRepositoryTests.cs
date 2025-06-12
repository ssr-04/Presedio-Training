using FluentAssertions;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Repositories;

namespace FreelanceProjectBoardApi.Tests.Repositories;

public class FreelancerProfileRepositoryTests : RepositoryTestBase
{
    private readonly FreelancerProfileRepository _freelancerProfileRepository;
    private static readonly Guid FreelancerUserId1 = Guid.NewGuid();
    private static readonly Guid FreelancerProfileId1 = Guid.NewGuid();

    public FreelancerProfileRepositoryTests()
    {
        _freelancerProfileRepository = new FreelancerProfileRepository(_context);
    }

    protected override void SeedDatabase()
    {
        var user1 = new User { Id = FreelancerUserId1, Email = "senior.dev@test.com", Type = UserType.Freelancer };
        var user2 = new User { Email = "junior.dev@test.com", Type = UserType.Freelancer };
        _context.Users.AddRange(user1, user2);

        var profiles = new List<FreelancerProfile>
        {
            new FreelancerProfile { Id = FreelancerProfileId1, UserId = user1.Id, User = user1, Headline = "Senior .NET Developer", ExperienceLevel = "Senior", HourlyRate = 100, IsAvailable = true, ProjectsCompleted = 20 },
            new FreelancerProfile { UserId = user2.Id, User = user2, Headline = "Junior React Developer", ExperienceLevel = "Junior", HourlyRate = 40, IsAvailable = false, ProjectsCompleted = 2 }
        };
        _context.FreelancerProfiles.AddRange(profiles);
    }

    [Fact]
    public async Task GetAllFreelancerProfilesAsync_FilterByExperience_ReturnsMatching()
    {
        var filter = new FreelancerFilter();
        filter.ExperienceLevel = "Senior";
        var result = await _freelancerProfileRepository.GetAllFreelancerProfilesAsync(filter, new PaginationParams());
        result.Data.Should().ContainSingle().Which.Headline.Should().Be("Senior .NET Developer");
    }

    [Fact]
    public async Task GetAllFreelancerProfilesAsync_FilterByAvailability_ReturnsMatching()
    {
        var filter = new FreelancerFilter();
        filter.IsAvailable = false;
        var result = await _freelancerProfileRepository.GetAllFreelancerProfilesAsync(filter, new PaginationParams());
        result.Data.Should().ContainSingle().Which.ExperienceLevel.Should().Be("Junior");
    }

    [Fact]
    public async Task GetAllFreelancerProfilesAsync_FilterByMinHourlyRate_ReturnsMatching()
    {
        var filter = new FreelancerFilter();
        filter.MinHourlyRate = 50;
        var result = await _freelancerProfileRepository.GetAllFreelancerProfilesAsync(filter, new PaginationParams());
        result.Data.Should().ContainSingle().Which.HourlyRate.Should().Be(100);
    }

    [Fact]
    public async Task GetFreelancerProfileDetailsAsync_WithValidId_ReturnsProfileWithUser()
    {
        var profile = await _freelancerProfileRepository.GetFreelancerProfileDetailsAsync(FreelancerProfileId1);
        profile.Should().NotBeNull();
        profile!.User.Should().NotBeNull();
        profile.User.Email.Should().Be("senior.dev@test.com");
    }

}