using FluentAssertions;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Repositories;

namespace FreelanceProjectBoardApi.Tests.Repositories;

public class FreelancerSkillRepositoryTests : RepositoryTestBase
{
    private readonly FreelancerSkillRepository _freelancerSkillRepository;
    private static readonly Guid FreelancerProfileId = Guid.NewGuid();
    private static readonly Guid SkillId1 = Guid.NewGuid();
    private static readonly Guid SkillId2 = Guid.NewGuid();
    private static readonly Guid UnrelatedSkillId = Guid.NewGuid();

    public FreelancerSkillRepositoryTests()
    {
        _freelancerSkillRepository = new FreelancerSkillRepository(_context);
    }

    protected override void SeedDatabase()
    {
        var user = new User { Email = "freelancer@test.com", Type = UserType.Freelancer };
        _context.Users.Add(user);

        var profile = new FreelancerProfile { Id = FreelancerProfileId, UserId = user.Id, User = user, Headline = "Skilled Dev" };
        _context.FreelancerProfiles.Add(profile);

        var skills = new List<Skill>
        {
            new Skill { Id = SkillId1, Name = "C#" },
            new Skill { Id = SkillId2, Name = "Docker" },
            new Skill { Id = UnrelatedSkillId, Name = "Marketing" }
        };
        _context.Skills.AddRange(skills);

        var freelancerSkills = new List<FreelancerSkill>
        {
            new FreelancerSkill { FreelancerProfileId = FreelancerProfileId, SkillId = SkillId1, FreelancerProfile = profile, Skill = skills[0] },
            new FreelancerSkill { FreelancerProfileId = FreelancerProfileId, SkillId = SkillId2, FreelancerProfile = profile, Skill = skills[1] }
        };
        _context.FreelancerSkills.AddRange(freelancerSkills);
    }

    [Fact]
    public async Task GetSkillsForFreelancerAsync_WithValidProfileId_ReturnsAllAssociatedSkills()
    {
        var results = await _freelancerSkillRepository.GetSkillsForFreelancerAsync(FreelancerProfileId);
        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results.Should().Contain(fs => fs.Skill.Name == "C#");
        results.Should().Contain(fs => fs.Skill.Name == "Docker");
        results.Should().NotContain(fs => fs.Skill.Name == "marketing");
    }

    [Fact]
    public async Task GetSkillsForFreelancerAsync_WithInvalidProfileId_ReturnsEmptyList()
    {
        var result = await _freelancerSkillRepository.GetSkillsForFreelancerAsync(Guid.NewGuid());
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FreelancerHasSkillAsync_WhenSkillIsAssociated_ReturnsTrue()
    {
        var result = await _freelancerSkillRepository.FreelancerHasSkillAsync(FreelancerProfileId, SkillId1);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task FreelancerHasSkillAsync_WhenSkillIsNotAssociated_ReturnsFalse()
    {
        var result = await _freelancerSkillRepository.FreelancerHasSkillAsync(FreelancerProfileId, UnrelatedSkillId);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task FreelancerHasSkillAsync_WithInvalidProfileId_ReturnsFalse()
    {
        var result = await _freelancerSkillRepository.FreelancerHasSkillAsync(Guid.NewGuid(), SkillId1);
        result.Should().BeFalse();
    }
}