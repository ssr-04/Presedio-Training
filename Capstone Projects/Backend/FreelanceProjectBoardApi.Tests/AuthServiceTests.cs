using Moq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using System.Linq.Expressions;
using FreelanceProjectBoardApi.Services.Implementations;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.DTOs.Auth;
using FreelanceProjectBoardApi.Services.Interfaces;


namespace FreelanceProjectBoardApi.Tests.Services
{
    public class AuthServiceTests
    {
        // Mocks for all dependencies of AuthService
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IClientProfileRepository> _clientProfileRepositoryMock;
        private readonly Mock<IFreelancerProfileRepository> _freelancerProfileRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IConfiguration> _configurationMock;

        private readonly IAuthService _authService;

        public AuthServiceTests()
        {
            // Instantiating mocks
            _userRepositoryMock = new Mock<IUserRepository>();
            _clientProfileRepositoryMock = new Mock<IClientProfileRepository>();
            _freelancerProfileRepositoryMock = new Mock<IFreelancerProfileRepository>();
            _mapperMock = new Mock<IMapper>();// dependency
            _configurationMock = new Mock<IConfiguration>();

            var jwtSettings = new Dictionary<string, string>
            {
                { "Jwt:Key", "A_VERY_SECURE_SECRET_KEY_FOR_TESTING_PURPOSES_123456" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:AccessTokenExpiryMinutes", "15" },
                { "Jwt:RefreshTokenExpiryDays", "7" }
            };

            // setting up IConfiguration to return the values from our dictionary
            _configurationMock.Setup(c => c[It.IsAny<string>()])
                              .Returns((string key) => jwtSettings[key]);

            // Instantiating the service with the mocked dependencies
            _authService = new AuthService(
                _userRepositoryMock.Object,
                _clientProfileRepositoryMock.Object,
                _freelancerProfileRepositoryMock.Object,
                _mapperMock.Object,
                _configurationMock.Object
            );
        }

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponseDto()
        {
            // Arrange
            var loginDto = new UserLoginDto { Email = "test@example.com", Password = "password123" };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = loginDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(loginDto.Password),
                Type = UserType.Freelancer
            };

            _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(loginDto.Email, false)).ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<AuthResponseDto>();
            result.Email.Should().Be(user.Email);
            result.UserId.Should().Be(user.Id.ToString());
            result.Token.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();

        }

        [Fact]
        public async Task LoginAsync_WithInvalidEmail_ShouldReturnNull()
        {
            // Arrange
            var loginDto = new UserLoginDto { Email = "nonexistent@test.com", Password = "password123" };
            _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(loginDto.Email, false)).ReturnsAsync((User?)null);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().BeNull();
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never); // because it doesn't make any change in db
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldReturnNull()
        {
            // Arrange
            var loginDto = new UserLoginDto { Email = "test@example.com", Password = "wrongpassword" };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = loginDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                Type = UserType.Client
            };

            _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(loginDto.Email, false)).ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().BeNull();
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_WithNewUser_ShouldSucceedAndReturnAuthResponseDto()
        {
            // Arrange
            var registerDto = new UserRegisterDto
            {
                Email = "newuser@example.com",
                Password = "password123",
                UserType = "Freelancer"
            };

            // User should not exist initially
            _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(registerDto.Email, false)).ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(new User());

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(registerDto.Email);
            result.UserType.Should().Be(registerDto.UserType);
            result.Token.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();

        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldReturnNull()
        {
            // Arrange
            var registerDto = new UserRegisterDto { Email = "existing@example.com", Password = "password123", UserType = "Client" };
            var existingUser = new User { Email = registerDto.Email };

            _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(registerDto.Email, false)).ReturnsAsync(existingUser);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().BeNull();
            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WithInvalidUserType_ShouldThrowArgumentException()
        {
            // Arrange
            var registerDto = new UserRegisterDto { Email = "test@example.com", Password = "password123", UserType = "InvalidType" };
            _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(registerDto.Email, false)).ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _authService.RegisterAsync(registerDto));
        }

        #endregion

        #region RefreshTokenAsync Tests

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewAuthResponseDto()
        {
            // Arrange
            var refreshToken = "valid-refresh-token";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "refresher@example.com",
                Type = UserType.Freelancer,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
            };
            var userList = new List<User> { user };

            _userRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<User, bool>>>(), null, "", false))
                .ReturnsAsync(userList);

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(user.Email);
            result.RefreshToken.Should().NotBe(refreshToken); // A NEW refresh token should be generated
            result.Token.Should().NotBeNullOrEmpty();

            _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Id == user.Id)), Times.Once);
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithInvalidOrExpiredToken_ShouldReturnNull()
        {
            // Arrange
            var refreshToken = "invalid-or-expired-token";

            // Simulate repository not finding any user with the given token (either non-existent or expired)
            _userRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<User, bool>>>(), null, "", false))
                .ReturnsAsync(new List<User>()); // Return an empty list

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken);

            // Assert
            result.Should().BeNull();
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region RevokeTokenAsync Tests

        [Fact]
        public async Task RevokeTokenAsync_WithValidToken_ShouldSucceedAndReturnTrue()
        {
            // Arrange
            var refreshToken = "token-to-revoke";
            var user = new User
            {
                Id = Guid.NewGuid(),
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
            };
            var userList = new List<User> { user };

            _userRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<User, bool>>>(), null, "", false))
                .ReturnsAsync(userList);

            // Act
            var result = await _authService.RevokeTokenAsync(refreshToken);

            // Assert
            result.Should().BeTrue();

            // Verify the user's token fields were cleared and saved
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u =>
                u.Id == user.Id &&
                u.RefreshToken == null &&
                u.RefreshTokenExpiryTime == DateTime.MinValue
            )), Times.Once);
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RevokeTokenAsync_WithInvalidToken_ShouldFailAndReturnFalse()
        {
            // Arrange
            var refreshToken = "non-existent-token";
            _userRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<User, bool>>>(), null, "", false))
                .ReturnsAsync(new List<User>()); // Return empty list

            // Act
            var result = await _authService.RevokeTokenAsync(refreshToken);

            // Assert
            result.Should().BeFalse();
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        #endregion
    }
}