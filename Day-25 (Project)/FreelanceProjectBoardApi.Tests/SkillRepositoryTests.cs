using FluentAssertions;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Repositories;

namespace FreelanceProjectBoardApi.Tests.Repositories;

public class SkillRepositoryTests : RepositoryTestBase
{
    private readonly SkillRepository _skillRepository;

    public SkillRepositoryTests()
    {
        _skillRepository = new SkillRepository(_context);
    }

    protected override void SeedDatabase()
    {
        _context.Skills.Add(new Skill { Name = "C#" });
    }

    [Fact]
    public async Task GetSkillByNameAsync_WithCaseInsensitiveName_ReturnsSkill()
    {
        var Skill = await _skillRepository.GetSkillByNameAsync("c#");
        Skill.Should().NotBeNull();
        Skill.Name.Should().Be("C#");
    }

}