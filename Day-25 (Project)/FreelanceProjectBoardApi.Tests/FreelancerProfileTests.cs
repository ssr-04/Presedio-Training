using Moq;
using FluentAssertions;
using AutoMapper;
using FreelanceProjectBoardApi.Services.Implementations;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.DTOs.FreelancerProfiles;
using FreelanceProjectBoardApi.Services.Interfaces;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using FreelanceProjectBoardApi.DTOs.Files;
using FreelanceProjectBoardApi.DTOs.Skills;

namespace FreelanceProjectBoardApi.Tests.Services
{
    public class FreelancerProfileServiceTests
    {
        // Mocks for all dependencies
        private readonly Mock<IFreelancerProfileRepository> _freelancerProfileRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ISkillRepository> _skillRepositoryMock;
        private readonly Mock<IFreelancerSkillRepository> _freelancerSkillRepositoryMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IRatingRepository> _ratingRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly IFreelancerProfileService _freelancerProfileService;

        public FreelancerProfileServiceTests()
        {
            // Instantiating mocks
            _freelancerProfileRepositoryMock = new Mock<IFreelancerProfileRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _skillRepositoryMock = new Mock<ISkillRepository>();
            _freelancerSkillRepositoryMock = new Mock<IFreelancerSkillRepository>();
            _fileServiceMock = new Mock<IFileService>();
            _ratingRepositoryMock = new Mock<IRatingRepository>();
            _mapperMock = new Mock<IMapper>();

            _freelancerProfileService = new FreelancerProfileService(
                _freelancerProfileRepositoryMock.Object,
                _userRepositoryMock.Object,
                _skillRepositoryMock.Object,
                _freelancerSkillRepositoryMock.Object,
                _fileServiceMock.Object,
                _ratingRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        #region CreateFreelancerProfileAsync Tests

        [Fact]
        public async Task CreateFreelancerProfileAsync_WithValidFreelancerUser_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Freelancer };
            var createDto = new CreateFreelancerProfileDto { Headline = "Test Dev" };
            var freelancerProfile = new FreelancerProfile { Id = Guid.NewGuid(), UserId = userId, Headline = "Test Dev", User = user };
            var responseDto = new FreelancerProfileResponseDto { Id = freelancerProfile.Id, Headline = "Test Dev" };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, false)).ReturnsAsync(user);
            _freelancerProfileRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<FreelancerProfile, bool>>>(), null, "", false))
                .ReturnsAsync(new List<FreelancerProfile>());
            _mapperMock.Setup(m => m.Map<FreelancerProfile>(createDto)).Returns(freelancerProfile);
            _freelancerProfileRepositoryMock.Setup(r => r.AddAsync(freelancerProfile)).ReturnsAsync(freelancerProfile);
            _freelancerProfileRepositoryMock.Setup(r => r.GetFreelancerProfileDetailsAsync(freelancerProfile.Id)).ReturnsAsync(freelancerProfile);
            _mapperMock.Setup(m => m.Map<FreelancerProfileResponseDto>(freelancerProfile)).Returns(responseDto);

            // Act
            var result = await _freelancerProfileService.CreateFreelancerProfileAsync(userId, createDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseDto);
            _freelancerProfileRepositoryMock.Verify(r => r.AddAsync(It.IsAny<FreelancerProfile>()), Times.Once);
            _freelancerProfileRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateFreelancerProfileAsync_WhenUserIsNotFreelancer_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Client }; // Not a Freelancer
            var createDto = new CreateFreelancerProfileDto { Headline = "Test Dev" };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, false)).ReturnsAsync(user);

            // Act
            var result = await _freelancerProfileService.CreateFreelancerProfileAsync(userId, createDto);

            // Assert
            result.Should().BeNull();
            _freelancerProfileRepositoryMock.Verify(r => r.AddAsync(It.IsAny<FreelancerProfile>()), Times.Never);
        }

        [Fact]
        public async Task CreateFreelancerProfileAsync_WhenProfileAlreadyExists_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Freelancer };
            var createDto = new CreateFreelancerProfileDto();
            var existingProfile = new List<FreelancerProfile> { new FreelancerProfile { UserId = userId, User = user } };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, false)).ReturnsAsync(user);
            _freelancerProfileRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<FreelancerProfile, bool>>>(), null, "", false))
                .ReturnsAsync(existingProfile);

            // Act
            var result = await _freelancerProfileService.CreateFreelancerProfileAsync(userId, createDto);

            // Assert
            result.Should().BeNull();
            _freelancerProfileRepositoryMock.Verify(r => r.AddAsync(It.IsAny<FreelancerProfile>()), Times.Never);
        }

        #endregion

        #region GetFreelancerProfileByIdAsync Tests

        [Fact]
        public async Task GetFreelancerProfileByIdAsync_WhenProfileExists_ShouldReturnDtoWithAverageRating()
        {
            // Arrange
            var profileId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Freelancer };
            var profile = new FreelancerProfile { Id = profileId, UserId = userId, User = user };
            var responseDto = new FreelancerProfileResponseDto { Id = profileId, UserId = userId };

            _freelancerProfileRepositoryMock.Setup(r => r.GetFreelancerProfileDetailsAsync(profileId)).ReturnsAsync(profile);
            _ratingRepositoryMock.Setup(r => r.GetAverageRatingForUserAsync(userId)).ReturnsAsync(4.5);
            _mapperMock.Setup(m => m.Map<FreelancerProfileResponseDto>(profile)).Returns(responseDto);

            // Act
            var result = await _freelancerProfileService.GetFreelancerProfileByIdAsync(profileId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseDto);
            result.AverageRating.Should().Be(4.5);
        }

        [Fact]
        public async Task GetFreelancerProfileByIdAsync_WhenProfileDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var profileId = Guid.NewGuid();
            _freelancerProfileRepositoryMock.Setup(r => r.GetFreelancerProfileDetailsAsync(profileId)).ReturnsAsync((FreelancerProfile?)null);

            // Act
            var result = await _freelancerProfileService.GetFreelancerProfileByIdAsync(profileId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region UpdateFreelancerProfileAsync Tests

        [Fact]
        public async Task UpdateFreelancerProfileAsync_WhenProfileExists_ShouldUpdateAndSucceed()
        {
            // Arrange
            var profileId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Freelancer };
            var profileToUpdate = new FreelancerProfile { Id = profileId, UserId = userId, User = user };
            var updateDto = new UpdateFreelancerProfileDto { Headline = "Updated Headline", Skills = new List<CreateSkillDto>() }; // Empty skills list for ease
            var responseDto = new FreelancerProfileResponseDto { Id = profileId, Headline = "Updated Headline" };

            _freelancerProfileRepositoryMock.Setup(r => r.GetFreelancerProfileDetailsAsync(profileId)).ReturnsAsync(profileToUpdate);
            _freelancerSkillRepositoryMock.Setup(r => r.GetSkillsForFreelancerAsync(profileId)).ReturnsAsync(new List<FreelancerSkill>());
            _freelancerProfileRepositoryMock.Setup(r => r.GetFreelancerProfileDetailsAsync(profileId)).ReturnsAsync(profileToUpdate); // For the reload
            _mapperMock.Setup(m => m.Map<FreelancerProfileResponseDto>(profileToUpdate)).Returns(responseDto);
            _ratingRepositoryMock.Setup(r => r.GetAverageRatingForUserAsync(userId)).ReturnsAsync(0);

            // Act
            var result = await _freelancerProfileService.UpdateFreelancerProfileAsync(profileId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Headline.Should().Be("Updated Headline");
            _mapperMock.Verify(m => m.Map(updateDto, profileToUpdate), Times.Once);
            _freelancerProfileRepositoryMock.Verify(r => r.UpdateAsync(profileToUpdate), Times.Once);
            _freelancerProfileRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        
        #endregion

        #region DeleteFreelancerProfileAsync Tests

        [Fact]
        public async Task DeleteFreelancerProfileAsync_WhenProfileExists_ShouldReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Freelancer };
            var profileId = Guid.NewGuid();
            var profile = new FreelancerProfile { Id = profileId, User = user };
            _freelancerProfileRepositoryMock.Setup(r => r.GetByIdAsync(profileId, false)).ReturnsAsync(profile);

            // Act
            var result = await _freelancerProfileService.DeleteFreelancerProfileAsync(profileId);

            // Assert
            result.Should().BeTrue();
            _freelancerProfileRepositoryMock.Verify(r => r.DeleteAsync(profileId), Times.Once);
            _freelancerProfileRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        
        #endregion

        #region Upload/Remove File Tests

        [Fact]
        public async Task UploadResumeAsync_WhenProfileExists_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Freelancer };
            var profileId = Guid.NewGuid();
            var profile = new FreelancerProfile { Id = profileId, UserId = userId, User=user};
            var mockFile = new Mock<IFormFile>();
            var fileResponseDto = new FileResponseDto { Id = Guid.NewGuid() };
            var profileResponseDto = new FreelancerProfileResponseDto { Id = profileId };

            _freelancerProfileRepositoryMock.Setup(r => r.GetByIdAsync(profileId, false)).ReturnsAsync(profile);
            _fileServiceMock.Setup(fs => fs.UploadFileAsync(mockFile.Object, userId, FileCategory.Resume, profileId)).ReturnsAsync(fileResponseDto);
            
            // Mock the final Get call
            _freelancerProfileRepositoryMock.Setup(r => r.GetFreelancerProfileDetailsAsync(profileId)).ReturnsAsync(profile);
            _mapperMock.Setup(m => m.Map<FreelancerProfileResponseDto>(profile)).Returns(profileResponseDto);


            // Act
            var result = await _freelancerProfileService.UploadResumeAsync(profileId, mockFile.Object);

            // Assert
            result.Should().NotBeNull();
            _freelancerProfileRepositoryMock.Verify(r => r.UpdateAsync(It.Is<FreelancerProfile>(p => p.ResumeFileId == fileResponseDto.Id)), Times.Once);
            _freelancerProfileRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveResumeAsync_WhenProfileAndFileExist_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Freelancer };
            var profileId = Guid.NewGuid();
            var fileId = Guid.NewGuid();
            var profile = new FreelancerProfile { Id = profileId, ResumeFileId = fileId, User = user };

            _freelancerProfileRepositoryMock.Setup(r => r.GetByIdAsync(profileId, false)).ReturnsAsync(profile);
            _fileServiceMock.Setup(fs => fs.DeleteFileAsync(fileId)).ReturnsAsync(true);

            // Act
            var result = await _freelancerProfileService.RemoveResumeAsync(profileId);

            // Assert
            result.Should().BeTrue();
            _freelancerProfileRepositoryMock.Verify(r => r.UpdateAsync(It.Is<FreelancerProfile>(p => p.ResumeFileId == null)), Times.Once);
            _fileServiceMock.Verify(fs => fs.DeleteFileAsync(fileId), Times.Once);
        }

        [Fact]
        public async Task RemoveResumeAsync_WhenProfileHasNoResume_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Type = UserType.Freelancer };
            var profileId = Guid.NewGuid();
            var profile = new FreelancerProfile { Id = profileId, ResumeFileId = null, User=user }; // No resume
            _freelancerProfileRepositoryMock.Setup(r => r.GetByIdAsync(profileId, false)).ReturnsAsync(profile);

            // Act
            var result = await _freelancerProfileService.RemoveResumeAsync(profileId);

            // Assert
            result.Should().BeFalse();
            _fileServiceMock.Verify(fs => fs.DeleteFileAsync(It.IsAny<Guid>()), Times.Never);
        }

        #endregion
    }
}