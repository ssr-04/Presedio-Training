using NUnit.Framework;
using Moq;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


// --- Unit Tests ---

[TestFixture]
public class DoctorServiceTests
{
    private Mock<IDoctorRepository> _mockDoctorRepository;
    private Mock<ISpecialityService> _mockSpecialityService;
    private Mock<IDoctorSpecialityService> _mockDoctorSpecialityService;
    private Mock<IOtherContextFunctionalities> _mockOtherContextFunctionalities;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IAuth0ManagementService> _mockAuth0ManagementService;
    private IMapper _mapper;
    private DoctorService _doctorService;

    [SetUp]
    public void Setup()
    {
        _mockDoctorRepository = new Mock<IDoctorRepository>();
        _mockSpecialityService = new Mock<ISpecialityService>();
        _mockDoctorSpecialityService = new Mock<IDoctorSpecialityService>();
        _mockOtherContextFunctionalities = new Mock<IOtherContextFunctionalities>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockAuth0ManagementService = new Mock<IAuth0ManagementService>();

        // Setup AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MappingProfiles());
        });
        _mapper = mapperConfig.CreateMapper();

        _doctorService = new DoctorService(
            _mockDoctorRepository.Object,
            _mockSpecialityService.Object,
            _mockDoctorSpecialityService.Object,
            _mockOtherContextFunctionalities.Object,
            _mockUserRepository.Object,
            _mockAuth0ManagementService.Object,
            _mapper
        );
    }

    // --- GetDoctorByIdAsync Tests ---
    [Test]
    public async Task GetDoctorByIdAsync_ExistingDoctor_ReturnsDoctorDto()
    {
        // Arrange
        var doctorId = 1;
        var doctor = new Doctor { Id = doctorId, Name = "Dr. John Doe", Email = "john.doe@example.com" };
        var expectedDto = new DoctorResponseDto { Id = doctorId, Name = "Dr. John Doe", Email = "john.doe@example.com" };

        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(doctorId, false))
            .ReturnsAsync(doctor);
        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(doctorId, true))
            .ReturnsAsync(doctor); // For includeDeleted = true

        // Act
        var result = await _doctorService.GetDoctorByIdAsync(doctorId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(expectedDto.Id));
        Assert.That(result.Name, Is.EqualTo(expectedDto.Name));
        _mockDoctorRepository.Verify(r => r.GetDoctorByIdAsync(doctorId, false), Times.Once);
    }

    [Test]
    public async Task GetDoctorByIdAsync_NonExistingDoctor_ReturnsNull()
    {
        // Arrange
        var doctorId = 99;
        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(doctorId, It.IsAny<bool>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var result = await _doctorService.GetDoctorByIdAsync(doctorId);

        // Assert
        Assert.That(result, Is.Null);
        _mockDoctorRepository.Verify(r => r.GetDoctorByIdAsync(doctorId, false), Times.Once);
    }

    [Test]
    public async Task GetDoctorByIdAsync_IncludeDeletedTrue_ReturnsDeletedDoctor()
    {
        // Arrange
        var doctorId = 2;
        var deletedDoctor = new Doctor { Id = doctorId, Name = "Dr. Jane Smith", Email = "jane.smith@example.com", IsDeleted = true };
        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(doctorId, true))
            .ReturnsAsync(deletedDoctor);

        // Act
        var result = await _doctorService.GetDoctorByIdAsync(doctorId, includeDeleted: true);

        // Assert
        Assert.That(result, Is.Not.Null);
        _mockDoctorRepository.Verify(r => r.GetDoctorByIdAsync(doctorId, true), Times.Once);
    }

    // --- GetAllDoctorsAsync Tests ---
    [Test]
    public async Task GetAllDoctorsAsync_ReturnsAllDoctors()
    {
        // Arrange
        var doctors = new List<Doctor>
        {
            new Doctor { Id = 1, Name = "Dr. One" },
            new Doctor { Id = 2, Name = "Dr. Two" }
        };
        _mockDoctorRepository.Setup(r => r.GetAllDoctorsAsync(false))
            .ReturnsAsync(doctors);

        // Act
        var result = await _doctorService.GetAllDoctorsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        _mockDoctorRepository.Verify(r => r.GetAllDoctorsAsync(false), Times.Once);
    }

    [Test]
    public async Task GetAllDoctorsAsync_NoDoctors_ReturnsEmptyList()
    {
        // Arrange
        _mockDoctorRepository.Setup(r => r.GetAllDoctorsAsync(It.IsAny<bool>()))
            .ReturnsAsync(new List<Doctor>());

        // Act
        var result = await _doctorService.GetAllDoctorsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
        _mockDoctorRepository.Verify(r => r.GetAllDoctorsAsync(false), Times.Once);
    }

    [Test]
    public async Task GetAllDoctorsAsync_IncludeDeletedTrue_ReturnsAllIncludingDeleted()
    {
        // Arrange
        var doctors = new List<Doctor>
        {
            new Doctor { Id = 1, Name = "Dr. Active" },
            new Doctor { Id = 2, Name = "Dr. Deleted", IsDeleted = true }
        };
        _mockDoctorRepository.Setup(r => r.GetAllDoctorsAsync(true))
            .ReturnsAsync(doctors);

        // Act
        var result = await _doctorService.GetAllDoctorsAsync(includeDeleted: true);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        _mockDoctorRepository.Verify(r => r.GetAllDoctorsAsync(true), Times.Once);
    }

    // --- AddDoctorAsync Tests ---
    // Additional tests added to address mapping and exception issues

    [TestFixture]
    public partial class DoctorServiceAddDoctorTests : DoctorServiceTests
    {
        [Test]
        public async Task AddDoctorAsync_SpecialityServiceThrowsException_PropagatesException_Updated()
        {
            // Arrange
            var doctorDto = new DoctorCreateDto
            {
                Name = "Dr. Error",
                Email = "error@example.com",
                Password = "errorPass",
                SpecialityNames = new List<SpecialityCreateDto>
                {
                    new SpecialityCreateDto { Name = "Radiology" }
                }
            };
            var createdDoctor = new Doctor { Id = 30, Name = "Dr. Error", Email = "error@example.com" };
            var createdAuth0User = new Auth0.ManagementApi.Models.User { UserId = "auth0|error", Email = "error@example.com" };

            _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(false);
            _mockAuth0ManagementService.Setup(s => s.GetAuth0UserByEmailAsync(doctorDto.Email))
                .ReturnsAsync((Auth0.ManagementApi.Models.User?)null);
            _mockAuth0ManagementService.Setup(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor"))
                .ReturnsAsync(createdAuth0User);
            _mockDoctorRepository.Setup(r => r.DoctorExistsByEmailAsync(doctorDto.Email, false))
                .ReturnsAsync(false);
            _mockDoctorRepository.Setup(r => r.AddDoctorAsync(It.IsAny<Doctor>())).ReturnsAsync(createdDoctor);
            _mockUserRepository.Setup(r => r.AddUserAsync(It.IsAny<User>())).ReturnsAsync(new User());
            // Since specialities are provided, but we want to simulate failure later,
            // we let GetAllSpecialitiesAsync succeed (return empty) and force the final fetch to return null.
            _mockSpecialityService.Setup(s => s.GetAllSpecialitiesAsync(It.IsAny<bool>()))
                .ReturnsAsync(new List<SpecialityResponseDto>());
            // Simulate GetDoctorByIdAsync failure after adding specialities
            _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(createdDoctor.Id, It.IsAny<bool>()))
                .ReturnsAsync((Doctor?)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _doctorService.AddDoctorAsync(doctorDto));
            Assert.That(ex.Message, Is.EqualTo($"Failed to retrieve newly created doctor with ID {createdDoctor.Id} after adding specialities."));
        }

        [Test]
        public void AutoMapper_Configuration_IsValid()
        {
            // Verify that AutoMapper configuration is valid.
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }

    [TestFixture]
    public class EnumerableExtensionsTests
    {
        [Test]
        public void DistinctBy_ExtensionMethod_Works_Correctly_DefaultComparer()
        {
            // Arrange
            var items = new[]
            {
                new { Id = 1, Name = "Alpha" },
                new { Id = 2, Name = "Beta" },
                new { Id = 1, Name = "Alpha" },
                new { Id = 3, Name = "Gamma" }
            };

            // Act
            var distinct = items.DistinctBy(x => x.Id).ToList();

            // Assert
            Assert.That(distinct.Count, Is.EqualTo(3));
            Assert.That(distinct.Any(x => x.Id == 1), Is.True);
            Assert.That(distinct.Any(x => x.Id == 2), Is.True);
            Assert.That(distinct.Any(x => x.Id == 3), Is.True);
        }

        [Test]
        public void DistinctBy_ExtensionMethod_Works_With_CustomComparer()
        {
            // Arrange
            var items = new[]
            {
                new Sample { Value = "A" },
                new Sample { Value = "a" },
                new Sample { Value = "B" },
                new Sample { Value = "b" }
            };

            // Act: using case-insensitive comparer on the Value property
            var distinct = items.DistinctBy(x => x.Value, StringComparer.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.That(distinct.Count, Is.EqualTo(2));
        }

        private class Sample
        {
            public string Value { get; set; } = string.Empty;
        }
    }

    [Test]
    public async Task AddDoctorAsync_UserAlreadyExistsLocally_ThrowsInvalidOperationException()
    {
        // Arrange
        var doctorDto = new DoctorCreateDto { Email = "existing@example.com" };
        _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _doctorService.AddDoctorAsync(doctorDto));
        Assert.That(ex!.Message, Is.EqualTo($"A user account with email '{doctorDto.Email}' already exists."));
        _mockUserRepository.Verify(r => r.UserExistsByUsernameAsync(doctorDto.Email), Times.Once);
        _mockAuth0ManagementService.Verify(s => s.GetAuth0UserByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task AddDoctorAsync_UserAlreadyExistsInAuth0_ThrowsInvalidOperationException()
    {
        // Arrange
        var doctorDto = new DoctorCreateDto { Email = "existing@example.com" };
        _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(false);
        _mockAuth0ManagementService.Setup(s => s.GetAuth0UserByEmailAsync(doctorDto.Email)).ReturnsAsync(new Auth0.ManagementApi.Models.User { Email = doctorDto.Email });

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _doctorService.AddDoctorAsync(doctorDto));
        Assert.That(ex!.Message, Is.EqualTo($"A user with email '{doctorDto.Email}' already exists in Auth0."));
        _mockUserRepository.Verify(r => r.UserExistsByUsernameAsync(doctorDto.Email), Times.Once);
        _mockAuth0ManagementService.Verify(s => s.GetAuth0UserByEmailAsync(doctorDto.Email), Times.Once);
        _mockAuth0ManagementService.Verify(s => s.CreateAuth0UserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task AddDoctorAsync_Auth0CreationFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var doctorDto = new DoctorCreateDto { Email = "new@example.com", Password = "pass" };
        _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(false);
        _mockAuth0ManagementService.Setup(s => s.GetAuth0UserByEmailAsync(doctorDto.Email)).ReturnsAsync((Auth0.ManagementApi.Models.User?)null);
        _mockAuth0ManagementService.Setup(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor"))
            .ThrowsAsync(new Exception("Auth0 API error"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _doctorService.AddDoctorAsync(doctorDto));
        Assert.That(ex!.Message, Does.Contain("Failed to create user in Auth0: Auth0 API error"));
        _mockUserRepository.Verify(r => r.UserExistsByUsernameAsync(doctorDto.Email), Times.Once);
        _mockAuth0ManagementService.Verify(s => s.GetAuth0UserByEmailAsync(doctorDto.Email), Times.Once);
        _mockAuth0ManagementService.Verify(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor"), Times.Once);
        _mockDoctorRepository.Verify(r => r.AddDoctorAsync(It.IsAny<Doctor>()), Times.Never);
    }

    [Test]
    public async Task AddDoctorAsync_DoctorEmailAlreadyExistsLocally_ThrowsInvalidOperationException()
    {
        // Arrange
        var doctorDto = new DoctorCreateDto { Email = "new@example.com", Password = "pass" };
        var createdAuth0User = new Auth0.ManagementApi.Models.User { UserId = "auth0|123", Email = "new@example.com" };

        _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(false);
        _mockAuth0ManagementService.Setup(s => s.GetAuth0UserByEmailAsync(doctorDto.Email)).ReturnsAsync((Auth0.ManagementApi.Models.User?)null);
        _mockAuth0ManagementService.Setup(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor")).ReturnsAsync(createdAuth0User);
        _mockDoctorRepository.Setup(r => r.DoctorExistsByEmailAsync(doctorDto.Email, false)).ReturnsAsync(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _doctorService.AddDoctorAsync(doctorDto));
        Assert.That(ex!.Message, Is.EqualTo($"Doctor with email '{doctorDto.Email}' already exists."));
        _mockUserRepository.Verify(r => r.UserExistsByUsernameAsync(doctorDto.Email), Times.Once);
        _mockAuth0ManagementService.Verify(s => s.GetAuth0UserByEmailAsync(doctorDto.Email), Times.Once);
        _mockAuth0ManagementService.Verify(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor"), Times.Once);
        _mockDoctorRepository.Verify(r => r.DoctorExistsByEmailAsync(doctorDto.Email, false), Times.Once);
        _mockDoctorRepository.Verify(r => r.AddDoctorAsync(It.IsAny<Doctor>()), Times.Never);
    }

    [Test]
    public async Task AddDoctorAsync_WithExistingSpecialities_Success()
    {
        // Arrange
        var doctorDto = new DoctorCreateDto
        {
            Name = "Dr. Test",
            Email = "test@example.com",
            Password = "password",
            SpecialityNames = new List<SpecialityCreateDto> { new SpecialityCreateDto { Name = "Cardiology" } }
        };
        var createdDoctor = new Doctor { Id = 10, Name = "Dr. Test", Email = "test@example.com" };
        var createdAuth0User = new Auth0.ManagementApi.Models.User { UserId = "auth0|abc", Email = "test@example.com" };
        var existingSpeciality = new SpecialityResponseDto 
        { 
            Id = 101, 
            Name = "Cardiology"
        };
        var finalDoctorResponseDto = new DoctorResponseDto { Id = 10, Name = "Dr. Test", Email = "test@example.com", Specialities = new List<SpecialityResponseDto> { existingSpeciality } };

        _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(false);
        _mockAuth0ManagementService.Setup(s => s.GetAuth0UserByEmailAsync(doctorDto.Email)).ReturnsAsync((Auth0.ManagementApi.Models.User?)null);
        _mockAuth0ManagementService.Setup(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor")).ReturnsAsync(createdAuth0User);
        _mockDoctorRepository.Setup(r => r.DoctorExistsByEmailAsync(doctorDto.Email, false)).ReturnsAsync(false);
        _mockDoctorRepository.Setup(r => r.AddDoctorAsync(It.IsAny<Doctor>())).ReturnsAsync(createdDoctor);
        _mockUserRepository.Setup(r => r.AddUserAsync(It.IsAny<User>())).ReturnsAsync(new User());
        _mockSpecialityService.Setup(s => s.GetAllSpecialitiesAsync(false)).ReturnsAsync(new List<SpecialityResponseDto> { existingSpeciality });
        _mockDoctorSpecialityService.Setup(s => s.AddDoctorSpecialityAsync(It.IsAny<DoctorSpecialityCreateDto>())).ReturnsAsync(new DoctorSpecialityResponseDto());

        // Mock GetDoctorByIdAsync to return the doctor with associated specialities for the final fetch
        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(createdDoctor.Id, It.IsAny<bool>()))
            .ReturnsAsync(new Doctor
            {
                Id = createdDoctor.Id,
                Name = createdDoctor.Name,
                Email = createdDoctor.Email,
                DoctorSpecialities = new List<DoctorSpeciality>
                {
                    new DoctorSpeciality { DoctorId = createdDoctor.Id, SpecialityId = existingSpeciality.Id, Speciality = _mapper.Map<Speciality>(existingSpeciality) }
                }
            });

        // Act
        var result = await _doctorService.AddDoctorAsync(doctorDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(finalDoctorResponseDto.Id));
        Assert.That(result.Name, Is.EqualTo(finalDoctorResponseDto.Name));
        Assert.That(result.Specialities, Is.Not.Null);
        Assert.That(result.Specialities!.Count, Is.EqualTo(1));
        Assert.That(result.Specialities.First().Name, Is.EqualTo("Cardiology"));

        _mockUserRepository.Verify(r => r.UserExistsByUsernameAsync(doctorDto.Email), Times.Once);
        _mockAuth0ManagementService.Verify(s => s.GetAuth0UserByEmailAsync(doctorDto.Email), Times.Once);
        _mockAuth0ManagementService.Verify(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor"), Times.Once);
        _mockDoctorRepository.Verify(r => r.DoctorExistsByEmailAsync(doctorDto.Email, false), Times.Once);
        _mockDoctorRepository.Verify(r => r.AddDoctorAsync(It.IsAny<Doctor>()), Times.Once);
        _mockUserRepository.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Once);
        _mockSpecialityService.Verify(s => s.GetAllSpecialitiesAsync(false), Times.Once);
        _mockSpecialityService.Verify(s => s.AddSpecialityAsync(It.IsAny<SpecialityCreateDto>()), Times.Never); // Should not add existing
        _mockDoctorSpecialityService.Verify(s => s.AddDoctorSpecialityAsync(It.Is<DoctorSpecialityCreateDto>(ds => ds.DoctorId == createdDoctor.Id && ds.SpecialityId == existingSpeciality.Id)), Times.Once);
        _mockDoctorRepository.Verify(r => r.GetDoctorByIdAsync(createdDoctor.Id, It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task AddDoctorAsync_WithNewSpecialities_Success()
    {
        // Arrange
        var doctorDto = new DoctorCreateDto
        {
            Name = "Dr. New",
            Email = "new_doc@example.com",
            Password = "password",
            SpecialityNames = new List<SpecialityCreateDto> { new SpecialityCreateDto { Name = "Pediatrics" } }
        };
        var createdDoctor = new Doctor { Id = 11, Name = "Dr. New", Email = "new_doc@example.com" };
        var createdAuth0User = new Auth0.ManagementApi.Models.User { UserId = "auth0|def", Email = "new_doc@example.com" };
        var newSpecialityResponse = new SpecialityResponseDto { Id = 102, Name = "Pediatrics" };

        _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(false);
        _mockAuth0ManagementService.Setup(s => s.GetAuth0UserByEmailAsync(doctorDto.Email)).ReturnsAsync((Auth0.ManagementApi.Models.User?)null);
        _mockAuth0ManagementService.Setup(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor")).ReturnsAsync(createdAuth0User);
        _mockDoctorRepository.Setup(r => r.DoctorExistsByEmailAsync(doctorDto.Email, false)).ReturnsAsync(false);
        _mockDoctorRepository.Setup(r => r.AddDoctorAsync(It.IsAny<Doctor>())).ReturnsAsync(createdDoctor);
        _mockUserRepository.Setup(r => r.AddUserAsync(It.IsAny<User>())).ReturnsAsync(new User());
        _mockSpecialityService.Setup(s => s.GetAllSpecialitiesAsync(false)).ReturnsAsync(new List<SpecialityResponseDto>()); // No existing specialities
        _mockSpecialityService.Setup(s => s.AddSpecialityAsync(It.IsAny<SpecialityCreateDto>())).ReturnsAsync(newSpecialityResponse);
        _mockDoctorSpecialityService.Setup(s => s.AddDoctorSpecialityAsync(It.IsAny<DoctorSpecialityCreateDto>())).ReturnsAsync(new DoctorSpecialityResponseDto());

        // Mock GetDoctorByIdAsync to return the doctor with associated specialities for the final fetch
        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(createdDoctor.Id, It.IsAny<bool>()))
            .ReturnsAsync(new Doctor
            {
                Id = createdDoctor.Id,
                Name = createdDoctor.Name,
                Email = createdDoctor.Email,
                DoctorSpecialities = new List<DoctorSpeciality>
                {
                    new DoctorSpeciality { DoctorId = createdDoctor.Id, SpecialityId = newSpecialityResponse.Id, Speciality = _mapper.Map<Speciality>(newSpecialityResponse) }
                }
            });

        // Act
        var result = await _doctorService.AddDoctorAsync(doctorDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(createdDoctor.Id));
        Assert.That(result.Specialities, Is.Not.Null);
        Assert.That(result.Specialities!.Count, Is.EqualTo(1));
        Assert.That(result.Specialities.First().Name, Is.EqualTo("Pediatrics"));

        _mockSpecialityService.Verify(s => s.GetAllSpecialitiesAsync(false), Times.Once);
        _mockSpecialityService.Verify(s => s.AddSpecialityAsync(It.Is<SpecialityCreateDto>(s => s.Name == "Pediatrics")), Times.Once);
        _mockDoctorSpecialityService.Verify(s => s.AddDoctorSpecialityAsync(It.Is<DoctorSpecialityCreateDto>(ds => ds.DoctorId == createdDoctor.Id && ds.SpecialityId == newSpecialityResponse.Id)), Times.Once);
    }

    [Test]
    public async Task AddDoctorAsync_WithMixedSpecialities_Success()
    {
        // Arrange
        var doctorDto = new DoctorCreateDto
        {
            Name = "Dr. Mixed",
            Email = "mixed@example.com",
            Password = "password",
            SpecialityNames = new List<SpecialityCreateDto>
            {
                new SpecialityCreateDto { Name = "Oncology" }, // Existing
                new SpecialityCreateDto { Name = "Dermatology" } // New
            }
        };
        var createdDoctor = new Doctor { Id = 12, Name = "Dr. Mixed", Email = "mixed@example.com" };
        var createdAuth0User = new Auth0.ManagementApi.Models.User { UserId = "auth0|ghi", Email = "mixed@example.com" };
        var existingSpeciality = new SpecialityResponseDto { Id = 201, Name = "Oncology" };
        var newSpecialityResponse = new SpecialityResponseDto { Id = 202, Name = "Dermatology" };

        _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(false);
        _mockAuth0ManagementService.Setup(s => s.GetAuth0UserByEmailAsync(doctorDto.Email)).ReturnsAsync((Auth0.ManagementApi.Models.User?)null);
        _mockAuth0ManagementService.Setup(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor")).ReturnsAsync(createdAuth0User);
        _mockDoctorRepository.Setup(r => r.DoctorExistsByEmailAsync(doctorDto.Email, false)).ReturnsAsync(false);
        _mockDoctorRepository.Setup(r => r.AddDoctorAsync(It.IsAny<Doctor>())).ReturnsAsync(createdDoctor);
        _mockUserRepository.Setup(r => r.AddUserAsync(It.IsAny<User>())).ReturnsAsync(new User());

        _mockSpecialityService.Setup(s => s.GetAllSpecialitiesAsync(false))
            .ReturnsAsync(new List<SpecialityResponseDto> { existingSpeciality }); // Only Oncology exists initially
        _mockSpecialityService.Setup(s => s.AddSpecialityAsync(It.Is<SpecialityCreateDto>(s => s.Name == "Dermatology")))
            .ReturnsAsync(newSpecialityResponse);
        _mockDoctorSpecialityService.Setup(s => s.AddDoctorSpecialityAsync(It.IsAny<DoctorSpecialityCreateDto>()))
            .ReturnsAsync(new DoctorSpecialityResponseDto());

        // Mock GetDoctorByIdAsync to return the doctor with associated specialities for the final fetch
        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(createdDoctor.Id, It.IsAny<bool>()))
            .ReturnsAsync(new Doctor
            {
                Id = createdDoctor.Id,
                Name = createdDoctor.Name,
                Email = createdDoctor.Email,
                DoctorSpecialities = new List<DoctorSpeciality>
                {
                    new DoctorSpeciality { DoctorId = createdDoctor.Id, SpecialityId = existingSpeciality.Id, Speciality = _mapper.Map<Speciality>(existingSpeciality) },
                    new DoctorSpeciality { DoctorId = createdDoctor.Id, SpecialityId = newSpecialityResponse.Id, Speciality = _mapper.Map<Speciality>(newSpecialityResponse) }
                }
            });

        // Act
        var result = await _doctorService.AddDoctorAsync(doctorDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(createdDoctor.Id));
        Assert.That(result.Specialities, Is.Not.Null);
        Assert.That(result.Specialities!.Count, Is.EqualTo(2));
        Assert.That(result.Specialities.Any(s => s.Name == "Oncology"), Is.True);
        Assert.That(result.Specialities.Any(s => s.Name == "Dermatology"), Is.True);

        _mockSpecialityService.Verify(s => s.GetAllSpecialitiesAsync(false), Times.Once);
        _mockSpecialityService.Verify(s => s.AddSpecialityAsync(It.Is<SpecialityCreateDto>(s => s.Name == "Dermatology")), Times.Once);
        _mockDoctorSpecialityService.Verify(s => s.AddDoctorSpecialityAsync(It.Is<DoctorSpecialityCreateDto>(ds => ds.DoctorId == createdDoctor.Id && ds.SpecialityId == existingSpeciality.Id)), Times.Once);
        _mockDoctorSpecialityService.Verify(s => s.AddDoctorSpecialityAsync(It.Is<DoctorSpecialityCreateDto>(ds => ds.DoctorId == createdDoctor.Id && ds.SpecialityId == newSpecialityResponse.Id)), Times.Once);
    }

    [Test]
    public async Task AddDoctorAsync_WithDuplicateSpecialitiesInInput_HandlesDistinctly()
    {
        // Arrange
        var doctorDto = new DoctorCreateDto
        {
            Name = "Dr. Dup",
            Email = "dup@example.com",
            Password = "password",
            SpecialityNames = new List<SpecialityCreateDto>
            {
                new SpecialityCreateDto { Name = "Neurology" },
                new SpecialityCreateDto { Name = "neurology" } // Duplicate, different casing
            }
        };
        var createdDoctor = new Doctor { Id = 13, Name = "Dr. Dup", Email = "dup@example.com" };
        var createdAuth0User = new Auth0.ManagementApi.Models.User { UserId = "auth0|jkl", Email = "dup@example.com" };
        var newSpecialityResponse = new SpecialityResponseDto { Id = 301, Name = "Neurology" };

        _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(false);
        _mockAuth0ManagementService.Setup(s => s.GetAuth0UserByEmailAsync(doctorDto.Email)).ReturnsAsync((Auth0.ManagementApi.Models.User?)null);
        _mockAuth0ManagementService.Setup(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor")).ReturnsAsync(createdAuth0User);
        _mockDoctorRepository.Setup(r => r.DoctorExistsByEmailAsync(doctorDto.Email, false)).ReturnsAsync(false);
        _mockDoctorRepository.Setup(r => r.AddDoctorAsync(It.IsAny<Doctor>())).ReturnsAsync(createdDoctor);
        _mockUserRepository.Setup(r => r.AddUserAsync(It.IsAny<User>())).ReturnsAsync(new User());

        _mockSpecialityService.Setup(s => s.GetAllSpecialitiesAsync(false)).ReturnsAsync(new List<SpecialityResponseDto>());
        _mockSpecialityService.Setup(s => s.AddSpecialityAsync(It.IsAny<SpecialityCreateDto>())).ReturnsAsync(newSpecialityResponse);
        _mockDoctorSpecialityService.Setup(s => s.AddDoctorSpecialityAsync(It.IsAny<DoctorSpecialityCreateDto>())).ReturnsAsync(new DoctorSpecialityResponseDto());

        // Mock GetDoctorByIdAsync to return the doctor with associated specialities for the final fetch
        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(createdDoctor.Id, It.IsAny<bool>()))
            .ReturnsAsync(new Doctor
            {
                Id = createdDoctor.Id,
                Name = createdDoctor.Name,
                Email = createdDoctor.Email,
                DoctorSpecialities = new List<DoctorSpeciality>
                {
                    new DoctorSpeciality { DoctorId = createdDoctor.Id, SpecialityId = newSpecialityResponse.Id, Speciality = _mapper.Map<Speciality>(newSpecialityResponse) }
                }
            });

        // Act
        var result = await _doctorService.AddDoctorAsync(doctorDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Specialities, Is.Not.Null);
        Assert.That(result.Specialities!.Count, Is.EqualTo(1)); // Only one speciality should be added
        Assert.That(result.Specialities.First().Name, Is.EqualTo("Neurology"));

        _mockSpecialityService.Verify(s => s.AddSpecialityAsync(It.Is<SpecialityCreateDto>(s => s.Name == "Neurology")), Times.Once); // Only added once
        _mockDoctorSpecialityService.Verify(s => s.AddDoctorSpecialityAsync(It.IsAny<DoctorSpecialityCreateDto>()), Times.Once); // Only associated once
    }

    [Test]
    public async Task AddDoctorAsync_WhenFinalDoctorFetchFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var doctorDto = new DoctorCreateDto
        {
            Name = "Dr. Fail",
            Email = "fail@example.com",
            Password = "password"
        };
        var createdDoctor = new Doctor { Id = 14, Name = "Dr. Fail", Email = "fail@example.com" };
        var createdAuth0User = new Auth0.ManagementApi.Models.User { UserId = "auth0|mno", Email = "fail@example.com" };

        _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(false);
        _mockAuth0ManagementService.Setup(s => s.GetAuth0UserByEmailAsync(doctorDto.Email)).ReturnsAsync((Auth0.ManagementApi.Models.User?)null);
        _mockAuth0ManagementService.Setup(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor")).ReturnsAsync(createdAuth0User);
        _mockDoctorRepository.Setup(r => r.DoctorExistsByEmailAsync(doctorDto.Email, false)).ReturnsAsync(false);
        _mockDoctorRepository.Setup(r => r.AddDoctorAsync(It.IsAny<Doctor>())).ReturnsAsync(createdDoctor);
        _mockUserRepository.Setup(r => r.AddUserAsync(It.IsAny<User>())).ReturnsAsync(new User());
        _mockSpecialityService.Setup(s => s.GetAllSpecialitiesAsync(It.IsAny<bool>())).ReturnsAsync(new List<SpecialityResponseDto>());
        // Simulate GetDoctorByIdAsync returning null after successful add
        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(createdDoctor.Id, It.IsAny<bool>())).ReturnsAsync((Doctor?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _doctorService.AddDoctorAsync(doctorDto));
        Assert.That(ex!.Message, Is.EqualTo($"Failed to retrieve newly created doctor with ID {createdDoctor.Id} after adding specialities."));

        _mockDoctorRepository.Verify(r => r.AddDoctorAsync(It.IsAny<Doctor>()), Times.Once);
        _mockUserRepository.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Once);
        _mockDoctorRepository.Verify(r => r.GetDoctorByIdAsync(createdDoctor.Id, It.IsAny<bool>()), Times.Once);
    }

    // --- UpdateDoctorAsync Tests ---
    [Test]
    public async Task UpdateDoctorAsync_ExistingDoctor_Success()
    {
        // Arrange
        var doctorId = 1;
        var existingDoctor = new Doctor { Id = doctorId, Name = "Old Name", Email = "old@example.com", Status = "Active" };
        var doctorDto = new DoctorUpdateDto { Name = "New Name", Email = "new@example.com", Status = "On Leave" };
        var updatedDoctor = new Doctor { Id = doctorId, Name = "New Name", Email = "new@example.com", Status = "On Leave" };
        var expectedDto = new DoctorResponseDto { Id = doctorId, Name = "New Name", Email = "new@example.com", Status = "On Leave" };

        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(doctorId, false)).ReturnsAsync(existingDoctor);
        _mockDoctorRepository.Setup(r => r.DoctorExistsByEmailAsync(doctorDto.Email, false)).ReturnsAsync(false); // Email changed, but new email doesn't exist
        _mockDoctorRepository.Setup(r => r.UpdateDoctorAsync(It.IsAny<Doctor>())).ReturnsAsync(updatedDoctor);

        // Act
        var result = await _doctorService.UpdateDoctorAsync(doctorId, doctorDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(expectedDto.Id));
        Assert.That(result.Name, Is.EqualTo(expectedDto.Name));
        Assert.That(result.Email, Is.EqualTo(expectedDto.Email));
        Assert.That(result.Status, Is.EqualTo(expectedDto.Status));

        _mockDoctorRepository.Verify(r => r.GetDoctorByIdAsync(doctorId, false), Times.Once);
        _mockDoctorRepository.Verify(r => r.DoctorExistsByEmailAsync(doctorDto.Email, false), Times.Once);
        _mockDoctorRepository.Verify(r => r.UpdateDoctorAsync(It.Is<Doctor>(d => d.Id == doctorId && d.Name == doctorDto.Name && d.Email == doctorDto.Email)), Times.Once);
    }

    [Test]
    public async Task UpdateDoctorAsync_DoctorNotFound_ReturnsNull()
    {
        // Arrange
        var doctorId = 99;
        var doctorDto = new DoctorUpdateDto { Name = "New Name" };
        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(doctorId, false)).ReturnsAsync((Doctor?)null);

        // Act
        var result = await _doctorService.UpdateDoctorAsync(doctorId, doctorDto);

        // Assert
        Assert.That(result, Is.Null);
        _mockDoctorRepository.Verify(r => r.GetDoctorByIdAsync(doctorId, false), Times.Once);
        _mockDoctorRepository.Verify(r => r.UpdateDoctorAsync(It.IsAny<Doctor>()), Times.Never);
    }

    [Test]
    public async Task UpdateDoctorAsync_EmailAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var doctorId = 1;
        var existingDoctor = new Doctor { Id = doctorId, Name = "Old Name", Email = "old@example.com" };
        var doctorDto = new DoctorUpdateDto { Name = "New Name", Email = "existing@example.com" };

        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(doctorId, false)).ReturnsAsync(existingDoctor);
        _mockDoctorRepository.Setup(r => r.DoctorExistsByEmailAsync(doctorDto.Email, false)).ReturnsAsync(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _doctorService.UpdateDoctorAsync(doctorId, doctorDto));
        Assert.That(ex!.Message, Is.EqualTo($"Doctor with email '{doctorDto.Email}' already exists."));

        _mockDoctorRepository.Verify(r => r.GetDoctorByIdAsync(doctorId, false), Times.Once);
        _mockDoctorRepository.Verify(r => r.DoctorExistsByEmailAsync(doctorDto.Email, false), Times.Once);
        _mockDoctorRepository.Verify(r => r.UpdateDoctorAsync(It.IsAny<Doctor>()), Times.Never);
    }

    // --- SoftDeleteDoctorAsync Tests ---
    [Test]
    public async Task SoftDeleteDoctorAsync_ExistingDoctor_Success()
    {
        // Arrange
        var doctorId = 1;
        var doctor = new Doctor { Id = doctorId, Name = "Dr. Delete", Email = "delete@example.com", IsDeleted = false };
        var user = new User { Id = 1, Username = "delete@example.com", DoctorId = doctorId, IsActive = true };

        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(doctorId, It.IsAny<bool>())).ReturnsAsync(doctor);
        _mockDoctorRepository.Setup(r => r.SoftDeleteDoctorAsync(doctorId)).ReturnsAsync(true);
        _mockUserRepository.Setup(r => r.GetUserByUsernameAsync(doctor.Email)).ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(user);

        // Act
        var result = await _doctorService.SoftDeleteDoctorAsync(doctorId);

        // Assert
        Assert.That(result, Is.True);
        _mockDoctorRepository.Verify(r => r.GetDoctorByIdAsync(doctorId, It.IsAny<bool>()), Times.Once);
        _mockDoctorRepository.Verify(r => r.SoftDeleteDoctorAsync(doctorId), Times.Once);
        _mockUserRepository.Verify(r => r.GetUserByUsernameAsync(doctor.Email), Times.Once);
        _mockUserRepository.Verify(r => r.UpdateUserAsync(It.Is<User>(u => u.IsActive == false && u.DoctorId == doctorId)), Times.Once);
    }

    [Test]
    public async Task SoftDeleteDoctorAsync_DoctorNotFound_ReturnsFalse()
    {
        // Arrange
        var doctorId = 99;
        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(doctorId, It.IsAny<bool>())).ReturnsAsync((Doctor?)null);

        // Act
        var result = await _doctorService.SoftDeleteDoctorAsync(doctorId);

        // Assert
        Assert.That(result, Is.False);
        _mockDoctorRepository.Verify(r => r.GetDoctorByIdAsync(doctorId, It.IsAny<bool>()), Times.Once);
        _mockDoctorRepository.Verify(r => r.SoftDeleteDoctorAsync(It.IsAny<int>()), Times.Never);
        _mockUserRepository.Verify(r => r.GetUserByUsernameAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SoftDeleteDoctorAsync_RepositorySoftDeleteFails_ReturnsFalse()
    {
        // Arrange
        var doctorId = 1;
        var doctor = new Doctor { Id = doctorId, Name = "Dr. Delete", Email = "delete@example.com", IsDeleted = false };

        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(doctorId, It.IsAny<bool>())).ReturnsAsync(doctor);
        _mockDoctorRepository.Setup(r => r.SoftDeleteDoctorAsync(doctorId)).ReturnsAsync(false); // Simulate failure

        // Act
        var result = await _doctorService.SoftDeleteDoctorAsync(doctorId);

        // Assert
        Assert.That(result, Is.False);
        _mockDoctorRepository.Verify(r => r.GetDoctorByIdAsync(doctorId, It.IsAny<bool>()), Times.Once);
        _mockDoctorRepository.Verify(r => r.SoftDeleteDoctorAsync(doctorId), Times.Once);
        _mockUserRepository.Verify(r => r.GetUserByUsernameAsync(It.IsAny<string>()), Times.Never); // User update should not be called
    }

    [Test]
    public async Task SoftDeleteDoctorAsync_UserNotFoundForDoctor_StillDeletesDoctor()
    {
        // Arrange
        var doctorId = 1;
        var doctor = new Doctor { Id = doctorId, Name = "Dr. Delete", Email = "delete@example.com", IsDeleted = false };

        _mockDoctorRepository.Setup(r => r.GetDoctorByIdAsync(doctorId, It.IsAny<bool>())).ReturnsAsync(doctor);
        _mockDoctorRepository.Setup(r => r.SoftDeleteDoctorAsync(doctorId)).ReturnsAsync(true);
        _mockUserRepository.Setup(r => r.GetUserByUsernameAsync(doctor.Email)).ReturnsAsync((User?)null); // User not found

        // Act
        var result = await _doctorService.SoftDeleteDoctorAsync(doctorId);

        // Assert
        Assert.That(result, Is.True); // Doctor should still be soft-deleted
        _mockDoctorRepository.Verify(r => r.SoftDeleteDoctorAsync(doctorId), Times.Once);
        _mockUserRepository.Verify(r => r.GetUserByUsernameAsync(doctor.Email), Times.Once);
        _mockUserRepository.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never); // User update should not be called
    }

    // --- DoctorExistsAsync Tests ---
    [TestCase(1, true, true)] // Exists, include deleted, expected true
    [TestCase(1, false, true)] // Exists, exclude deleted, expected true
    [TestCase(2, true, false)] // Does not exist, include deleted, expected false
    [TestCase(2, false, false)] // Does not exist, exclude deleted, expected false
    public async Task DoctorExistsAsync_ReturnsCorrectResult(int id, bool includeDeleted, bool expected)
    {
        // Arrange
        _mockDoctorRepository.Setup(r => r.DoctorExistsAsync(id, includeDeleted)).ReturnsAsync(expected);

        // Act
        var result = await _doctorService.DoctorExistsAsync(id, includeDeleted);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
        _mockDoctorRepository.Verify(r => r.DoctorExistsAsync(id, includeDeleted), Times.Once);
    }

    // --- DoctorExistsByEmailAsync Tests ---
    [TestCase("test@example.com", true, true)]
    [TestCase("test@example.com", false, true)]
    [TestCase("nonexistent@example.com", true, false)]
    [TestCase("nonexistent@example.com", false, false)]
    public async Task DoctorExistsByEmailAsync_ReturnsCorrectResult(string email, bool includeDeleted, bool expected)
    {
        // Arrange
        _mockDoctorRepository.Setup(r => r.DoctorExistsByEmailAsync(email, includeDeleted)).ReturnsAsync(expected);

        // Act
        var result = await _doctorService.DoctorExistsByEmailAsync(email, includeDeleted);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
        _mockDoctorRepository.Verify(r => r.DoctorExistsByEmailAsync(email, includeDeleted), Times.Once);
    }

    // --- GetDoctorsBySpecialityNameAsync Tests ---
    [Test]
    public async Task GetDoctorsBySpecialityNameAsync_ReturnsMappedDoctors()
    {
        // Arrange
        var specialityName = "Cardiology";
        var doctorsFromSp = new List<Doctor>
        {
            new Doctor { Id = 1, Name = "Dr. Heart", Email = "heart@example.com" },
            new Doctor { Id = 2, Name = "Dr. Beat", Email = "beat@example.com" }
        };
        var expectedDtos = new List<DoctorResponseDto>
        {
            new DoctorResponseDto { Id = 1, Name = "Dr. Heart", Email = "heart@example.com" },
            new DoctorResponseDto { Id = 2, Name = "Dr. Beat", Email = "beat@example.com" }
        };

        _mockOtherContextFunctionalities.Setup(o => o.GetDoctorsBySpecialityNameFromSpAsync(specialityName))
            .ReturnsAsync(doctorsFromSp);

        // Act
        var result = await _doctorService.GetDoctorsBySpecialityNameAsync(specialityName);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(expectedDtos.Count));
        Assert.That(result.Any(d => d.Name == "Dr. Heart"), Is.True);
        Assert.That(result.Any(d => d.Name == "Dr. Beat"), Is.True);
        _mockOtherContextFunctionalities.Verify(o => o.GetDoctorsBySpecialityNameFromSpAsync(specialityName), Times.Once);
    }

    [Test]
    public async Task GetDoctorsBySpecialityNameAsync_NoDoctorsFound_ReturnsEmptyList()
    {
        // Arrange
        var specialityName = "NonExistent";
        _mockOtherContextFunctionalities.Setup(o => o.GetDoctorsBySpecialityNameFromSpAsync(specialityName))
            .ReturnsAsync(new List<Doctor>());

        // Act
        var result = await _doctorService.GetDoctorsBySpecialityNameAsync(specialityName);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
        _mockOtherContextFunctionalities.Verify(o => o.GetDoctorsBySpecialityNameFromSpAsync(specialityName), Times.Once);
    }

}

// Extension method for DistinctBy, as it's not available in all .NET versions by default
public static class EnumerableExtensions
{
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        HashSet<TKey> seenKeys = new HashSet<TKey>();
        foreach (TSource element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        HashSet<TKey> seenKeys = new HashSet<TKey>(comparer);
        foreach (TSource element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }
}
