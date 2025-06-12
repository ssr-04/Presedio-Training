using FluentAssertions;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Repositories;

namespace FreelanceProjectBoardApi.Tests.Repositories;

public class FileRepositoryTests : RepositoryTestBase
{
    private readonly FileRepository _fileRepository;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid FileId = Guid.NewGuid();

    public FileRepositoryTests()
    {
        _fileRepository = new FileRepository(_context);
    }

    protected override void SeedDatabase()
    {
        var user = new User { Id = UserId, Email = "uploader@test.com" };
        _context.Users.Add(user);

        var file = new Models.File
        {
            Id = FileId,
            FileName = "test-document.pdf",
            StoredFileName = $"{FileId}.pdf",
            FilePath = "/uploads/",
            MimeType = "application/pdf",
            UploaderId = UserId
        };
        _context.Files.Add(file);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCalledWithValidId_ReturnsFile()
    {
        var result = await _fileRepository.GetByIdAsync(FileId);
        result.Should().NotBeNull();
        result!.FileName.Should().Be("test-document.pdf");
        result.UploaderId.Should().Be(UserId);
    }

    [Fact]
    public async Task AddAsync_WhenCalled_AddsFileToContext()
    {
        var newFile = new Models.File
        {
            FileName = "new-file.jpg",
            StoredFileName = $"{Guid.NewGuid()}.jpg",
            FilePath = "/uploads/",
            MimeType = "image/jpg",
            UploaderId = UserId
        };

        await _fileRepository.AddAsync(newFile);
        await _context.SaveChangesAsync();

        var result = await _fileRepository.GetByIdAsync(newFile.Id);
        result.Should().NotBeNull();
        result!.Id.Should().Be(newFile.Id);
    }
}