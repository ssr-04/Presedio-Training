using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Repositories;
using FluentAssertions;

namespace FreelanceProjectBoardApi.Tests.Repositories;

public class GenericRepositoryTests : RepositoryTestBase
{
    private readonly GenericRepository<Skill> _genericRepo;
    private static readonly Guid SkillId = Guid.NewGuid();

    public GenericRepositoryTests()
    {
        _genericRepo = new GenericRepository<Skill>(_context);
    }

    protected override void SeedDatabase()
    {
        _context.Skills.Add(new Skill { Id = SkillId, Name = "Test Skill" });
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsEntity()
    {
        var result = await _genericRepo.GetByIdAsync(SkillId);
        result!.Should().NotBeNull();
        result!.Name.Should().Be("Test Skill");
    }

    [Fact]
    public async Task AddAsync_ShouldAddNewEntity()
    {
        var newSkill = new Skill { Name = "New Skill" };
        await _genericRepo.AddAsync(newSkill);
        await _context.SaveChangesAsync();

        var result = await _genericRepo.GetByIdAsync(newSkill.Id);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyEntity()
    {
        var skill = await _genericRepo.GetByIdAsync(SkillId);
        skill!.Name = "Updated Skill";

        await _genericRepo.UpdateAsync(skill);
        await _context.SaveChangesAsync();

        var updatedSkill = await _genericRepo.GetByIdAsync(SkillId);
        updatedSkill!.Name.Should().Be("Updated Skill");
        updatedSkill.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldPerformSoftDelete()
    {
        await _genericRepo.DeleteAsync(SkillId);
        await _context.SaveChangesAsync();

        var result = await _genericRepo.GetByIdAsync(SkillId);
        result.Should().BeNull();

        var resultWithDeleted = await _genericRepo.GetByIdAsync(SkillId, includeDeleted: true);
        resultWithDeleted.Should().NotBeNull();
        resultWithDeleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task HardDeleteAsync_ShouldPermanentlyRemoveEntity()
    {
        await _genericRepo.HardDeleteAsync(SkillId);
        await _context.SaveChangesAsync();

        var result = await _genericRepo.GetByIdAsync(SkillId, includeDeleted: true);
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        var count = await _genericRepo.CountAsync();
        count.Should().Be(1);
    }
}

