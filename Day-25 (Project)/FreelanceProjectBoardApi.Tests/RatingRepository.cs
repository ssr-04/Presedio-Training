using FluentAssertions;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Repositories;

namespace FreelanceProjectBoardApi.Tests.Repositories;

public class RatingRepositoryTests : RepositoryTestBase
{
    private readonly RatingRepository _ratingRepository;
    private static readonly Guid ClientId = Guid.NewGuid();
    private static readonly Guid FreelancerId = Guid.NewGuid();
    private static readonly Guid ProjectId = Guid.NewGuid();

    public RatingRepositoryTests()
    {
        _ratingRepository = new RatingRepository(_context);
    }

    protected override void SeedDatabase()
    {
        var client = new User { Id = ClientId, Email = "client@test.com", Type = UserType.Client };
        var freelancer = new User { Id = FreelancerId, Email = "freelancer@test.com", Type = UserType.Freelancer };
        _context.Users.AddRange(client, freelancer);

        var project = new Project { Id = ProjectId, Title = "Test Project", ClientId = ClientId, Client = client };
        _context.Projects.Add(project);

        // Client rates Freelancer
        _context.Ratings.Add(new Rating { ProjectId = ProjectId, RaterId = ClientId, RateeId = FreelancerId, RatingValue = 5 });
        // Another rating for the same Freelancer (something random :)
        _context.Ratings.Add(new Rating { ProjectId = Guid.NewGuid(), RaterId = Guid.NewGuid(), RateeId = FreelancerId, RatingValue = 3 });
    }

    [Fact]
    public async Task GetAverageRatingForUserAsync_WithValidUserId_CalculatesCorrectAverage()
    {
        var avg = await _ratingRepository.GetAverageRatingForUserAsync(FreelancerId);
        avg.Should().Be(4.0); //5+3
    }

    [Fact]
    public async Task GetRatingsReceivedByUserAsync_WithValidUserId_ReturnsRatings()
    {
        var ratings = await _ratingRepository.GetRatingsReceivedByUserAsync(FreelancerId);
        ratings.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRatingsGivenByUserAsync_WithValidUserId_ReturnsRatings()
    {
        var ratings = await _ratingRepository.GetRatingsGivenByUserAsync(ClientId);
        ratings.Should().HaveCount(1);
        ratings.First().RatingValue.Should().Be(5);
    }

    [Fact]
    public async Task GetAverageRatingForProjectAsync_WithValidProjectId_ReturnsRating()
    {
        var avg = await _ratingRepository.GetAverageRatingForProjectAsync(ProjectId);
        avg.Should().Be(5);
    }
}